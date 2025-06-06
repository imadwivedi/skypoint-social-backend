using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyPointSocial.Core.ClientModels.Post;
using SkyPointSocial.Core.Interfaces;
using System.Security.Claims;

namespace SkyPointSocial.API.Controllers
{
    [ApiController]
    [Route("api")]
    [ProducesResponseType(typeof(PostClientModel), StatusCodes.Status200OK)]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostController> _logger;

        public PostController(IPostService postService, ILogger<PostController> logger)
        {
            _postService = postService;
            _logger = logger;
        }

        [HttpPost("post")]
        [Authorize]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostClientModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                var post = await _postService.CreateAsync(userId, model);
                return Ok(post);
            }
            catch(ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid post content");
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized post creation attempt");
                return Unauthorized(new { error = "You must be logged in to create a post" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post");
                return StatusCode(500, new { error = "An error occurred while creating the post" });
            }
        }

        [HttpGet("posts/user/{userId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetUserPosts(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var posts = await _postService.GetByUserIdAsync(userId, GetCurrentUserId(), page, pageSize);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting posts for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while fetching user posts" });
            }
        }


        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return Guid.Parse(userIdClaim);
        }
    }
}