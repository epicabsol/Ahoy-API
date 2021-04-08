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
        private ILogger<PostsController> Logger { get; }

        public PostsController(DataService dataService, ILogger<PostsController> logger)
        {
            this.DataService = dataService;
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
            Models.Post post = this.DataService.CreatePost(request.Author, request.Content);
            return this.CreateSuccess(post);
        }

        // GET: api/posts/before?end=18&count=10
        [HttpGet("before")]
        public IActionResult GetPostsBefore(int end = -1, int count = 10)
        {
            // TODO: Implement!
            throw new NotImplementedException();
        }

        // GET: api/posts/after?start=18&count=10
        [HttpGet("after")]
        public IActionResult GetPostsAfter(int start = -1, int count = 10)
        {
            // TODO: Implement!
            throw new NotImplementedException();
        }

        // GET: api/posts/notifications
        [HttpGet("notifications")]
        public IActionResult SubscribeToNotifications()
        {
            // TODO: Implement!
            throw new NotImplementedException();
        }
    }
}
