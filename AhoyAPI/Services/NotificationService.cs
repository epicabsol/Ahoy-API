using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace AhoyAPI.Services
{
    public class NotificationService
    {
        private const int HeartbeatInterval = 45; // In seconds

        private IHostApplicationLifetime HostLifetime { get; }
        private ReaderWriterLockSlim _subscriberListLock = new ReaderWriterLockSlim();
        private List<HttpResponse> Subscribers { get; } = new List<HttpResponse>();

        private System.Text.Json.JsonSerializerOptions SerializerOptions { get; } = new System.Text.Json.JsonSerializerOptions() { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };

        public NotificationService(IHostApplicationLifetime hostLifetime)
        {
            this.HostLifetime = hostLifetime;
        }

        public void PublishEvent(string eventName, object data)
        {
            this._subscriberListLock.EnterReadLock();
            foreach (HttpResponse response in this.Subscribers)
            {
                lock (response)
                {
                    if (!String.IsNullOrEmpty(eventName))
                    {
                        response.WriteAsync($"event: {eventName}\n").Wait();
                    }
                    response.WriteAsync($"data: {System.Text.Json.JsonSerializer.Serialize(data, this.SerializerOptions)}").Wait();
                    response.Body.FlushAsync().Wait();
                }
            }
            this._subscriberListLock.ExitReadLock();
        }

        public void PublishPostCreated(Models.Post post)
        {
            this.PublishEvent("postCreated", post);
        }

        private void Unsubscribe(HttpResponse response, CancellationTokenSource heartbeatCancellationSource)
        {
            // Remove the response from the subsciber list
            this._subscriberListLock.EnterWriteLock();
            this.Subscribers.Remove(response);
            this._subscriberListLock.ExitWriteLock();

            // Cancel the connection's thread's sleep between heartbeats
            heartbeatCancellationSource.Cancel();
        }

        public async Task Subscribe(HttpResponse response)
        {
            CancellationTokenSource heartbeatCancellationSource = new CancellationTokenSource();

            response.HttpContext.RequestAborted.Register(() => this.Unsubscribe(response, heartbeatCancellationSource));
            this.HostLifetime.ApplicationStopping.Register(() => this.Unsubscribe(response, heartbeatCancellationSource));

            this._subscriberListLock.EnterWriteLock();
            this.Subscribers.Add(response);
            this._subscriberListLock.ExitWriteLock();

            while (!heartbeatCancellationSource.IsCancellationRequested)
            {
                lock (response)
                {
                    response.WriteAsync(":\n\n").Wait();
                }
                try
                {
                    await Task.Delay(HeartbeatInterval * 1000, heartbeatCancellationSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // No worries, we'll just exit the loop this time.
                }
            }
        }
    }
}
