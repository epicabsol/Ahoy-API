using System;
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
