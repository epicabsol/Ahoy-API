using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace AhoyAPI.Services
{
    public class DataServiceOptions
    {
        public const string SectionName = "DataService";

        public string Host { get; set; }
        public string Database { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class DataService : IDisposable
    {
        private DataServiceOptions Options { get; }
        private ILogger<DataService> Logger { get; }

        private string ConnectionString => $"Host={this.Options.Host};Username={this.Options.Username};Password={this.Options.Password};Database={this.Options.Database}";
        private NpgsqlConnection _connection = null;
        private NpgsqlConnection Connection
        {
            get
            {
                if (this._connection == null)
                {
                    this._connection = new NpgsqlConnection(this.ConnectionString);
                    this._connection.Open();
                }
                return this._connection;
            }
        }

        public DataService(IOptions<DataServiceOptions> options, ILogger<DataService> logger)
        {
            this.Options = options.Value;
            this.Logger = logger;
        }

        public Models.Post CreatePost(string author, string content)
        {
            using (NpgsqlCommand command = new NpgsqlCommand("INSERT INTO Post (Author, Content) VALUES (@author, @content) RETURNING ID, Author, Content", this.Connection))
            {
                command.Parameters.AddWithValue("@author", author);
                command.Parameters.AddWithValue("@content", content);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        return new Models.Post(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public List<Models.Post> GetPostsBefore(int endID, int maxCount, out bool more)
        {
            string query = "SELECT ID, Author, Content FROM Post";
            if (endID != -1)
            {
                query += " WHERE ID < @endID";
            }
            query += " ORDER BY ID DESC LIMIT @maxCount";
            using (NpgsqlCommand command = new NpgsqlCommand(query, this.Connection))
            {
                if (endID != -1)
                {
                    command.Parameters.AddWithValue("@endID", endID);
                }
                command.Parameters.AddWithValue("@maxCount", maxCount + 1);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    List<Models.Post> posts = new List<Models.Post>(maxCount);
                    while (reader.Read())
                    {
                        posts.Add(new Models.Post(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                    }

                    // We intentionally limited the query to one more than the requested max. If we got the extra row, mark
                    // the `more` flag as true.
                    if (posts.Count > maxCount)
                    {
                        posts.RemoveAt(posts.Count - 1);
                        more = true;
                    }
                    else
                    {
                        more = false;
                    }

                    return posts;
                }
            }
        }

        public List<Models.Post> GetPostsAfter(int startID, int maxCount, out bool more)
        {
            string query = "SELECT ID, Author, Content FROM Post";
            if (startID != -1)
            {
                query += " WHERE ID > @startID";
            }
            query += " ORDER BY ID ASC LIMIT @maxCount";
            using (NpgsqlCommand command = new NpgsqlCommand(query, this.Connection))
            {
                if (startID != -1)
                {
                    command.Parameters.AddWithValue("@startID", startID);
                }
                command.Parameters.AddWithValue("@maxCount", maxCount + 1);
                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    List<Models.Post> posts = new List<Models.Post>(maxCount);
                    while (reader.Read())
                    {
                        posts.Add(new Models.Post(reader.GetInt32(0), reader.GetString(1), reader.GetString(2)));
                    }

                    // We intentionally limited the query to one more than the requested max. If we got the extra row, mark
                    // the `more` flag as true.
                    if (posts.Count > maxCount)
                    {
                        posts.RemoveAt(posts.Count - 1);
                        more = true;
                    }
                    else
                    {
                        more = false;
                    }

                    return posts;
                }
            }
        }

        public void Dispose()
        {
            if (this._connection != null)
            {
                this._connection.Close();
                this._connection.Dispose();
                this._connection = null;
            }
        }
    }
}
