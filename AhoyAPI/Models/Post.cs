using System;
namespace AhoyAPI.Models
{
    public class Post
    {
        public const int MaxAuthorLength = 50;
        public const int MaxContentLength = 280;

        public int ID { get; }
        public string Author { get; set; }
        public string Content { get; set; }

        public Post(int id, string author, string content)
        {
            this.ID = id;
            this.Author = author;
            this.Content = content;
        }
    }
}
