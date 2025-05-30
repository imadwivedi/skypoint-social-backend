using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyPointSocial.Core.ClientModels.Comment;
using SkyPointSocial.Core.Interfaces;
using System.Security.Claims;

namespace SkyPointSocial.API.Controllers
{
    [ApiController]
    [Route("api")]
    [ProducesResponseType(typeof(CommentClientModel), StatusCodes.Status200OK)]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ILogger<CommentController> _logger;

        public CommentController(ICommentService commentService, ILogger<CommentController> logger)
        {
            _commentService = commentService;
            _logger = logger;
        }

        [HttpPost("comment/{postId:guid}")]
        [Authorize]
        public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentClientModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Create a new model with the userId and postId from the route
                var commentModel = new CreateCommentClientModel
                {
                    PostId = postId,
                    Content = model.Content,
                    ParentCommentId = model.ParentCommentId
                };
                
                var comment = await _commentService.CreateAsync(userId, commentModel);
                return Ok(comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                return StatusCode(500, new { error = "An error occurred while creating comment" });
            }
        }

        [HttpGet("comment/{postId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetComments(Guid postId)
        {
            try
            {
                var comments = await _commentService.GetByPostIdAsync(postId);
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for post {PostId}", postId);
                return StatusCode(500, new { error = "An error occurred while fetching comments" });
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