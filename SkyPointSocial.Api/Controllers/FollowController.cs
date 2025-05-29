using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyPointSocial.Core.ClientModels.Follow;
using SkyPointSocial.Core.Interfaces;
using System.Security.Claims;

namespace SkyPointSocial.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;
        private readonly ILogger<FollowController> _logger;

        public FollowController(IFollowService followService, ILogger<FollowController> logger)
        {
            _followService = followService;
            _logger = logger;
        }

        [HttpPost("follow")]
        [Authorize]
        public async Task<IActionResult> Follow([FromBody] FollowClientModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                var isFollowing = await _followService.IsFollowingAsync(userId, model.UserId);
                if (!isFollowing)
                {
                    // Not following, so follow
                    await _followService.FollowAsync(userId, model.UserId);
                    return Ok(new { message = "Successfully followed user" });
                }
                else
                {
                    await _followService.UnfollowAsync(userId, model.UserId);
                    return Ok(new { message = "Successfully unfollowed user" });
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in follow/unfollow");
                return StatusCode(500, new { error = "An error occurred" });
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