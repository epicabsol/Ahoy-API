using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AhoyAPI.Services;

namespace AhoyAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : Controller
    {
        private DataService DataService { get; }
        private NotificationService NotificationService { get; }
        private ILogger<PostsController> Logger { get; }

        public PostsController(DataService dataService, NotificationService notificactionService, ILogger<PostsController> logger)
        {
            this.DataService = dataService;
            this.NotificationService = notificactionService;
            this.Logger = logger;
        }

        // POST api/posts
        public class CreatePostRequest
        {
            public string Author { get; set; }
            public string Content { get; set; }
        }
        [HttpPost]
        public IActionResult CreatePost(CreatePostRequest request)
        {
            if (request.Author.Length > Models.Post.MaxAuthorLength)
            {
                return this.CreateFailure($"Author names cannot be longer than {Models.Post.MaxAuthorLength} characters.");
            }
            else if (request.Content.Length > Models.Post.MaxContentLength)
            {
                return this.CreateFailure($"Post content cannot be longer than {Models.Post.MaxContentLength} characters.");
            }
            else
            {
                Models.Post post = this.DataService.CreatePost(request.Author, request.Content);

                this.NotificationService.PublishPostCreated(post);

                return this.CreateSuccess(post);
            }
        }

        public class PostListResponse
        {
            public List<Models.Post> Posts { get; set; }
            public bool More { get; set; }
        }

        // GET: api/posts/before?end=18&count=10
        [HttpGet("before")]
        public IActionResult GetPostsBefore(int end = -1, int count = 10)
        {
            PostListResponse response = new PostListResponse();
            response.Posts = this.DataService.GetPostsBefore(end, count, out bool more);
            response.More = more;
            return this.CreateSuccess(response);
        }

        // GET: api/posts/after?start=18&count=10
        [HttpGet("after")]
        public IActionResult GetPostsAfter(int start = -1, int count = 10)
        {
            PostListResponse response = new PostListResponse();
            response.Posts = this.DataService.GetPostsAfter(start, count, out bool more);
            response.More = more;
            return this.CreateSuccess(response);
        }

        // GET: api/posts/notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> SubscribeToNotifications()
        {
            this.Response.Headers.Add("Content-Type", "text/event-stream");
            this.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status200OK;

            // This never returns
            await this.NotificationService.Subscribe(this.Response);

            return Ok();
        }
    }
}
