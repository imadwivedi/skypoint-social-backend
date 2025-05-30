using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyPointSocial.Core.ClientModels.Feed;
using SkyPointSocial.Core.Interfaces;
using System.Security.Claims;

namespace SkyPointSocial.API.Controllers
{
    [ApiController]
    [Route("api")]
    [ProducesResponseType(typeof(FeedResponseClientModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class FeedController : ControllerBase
    {
        private readonly IFeedService _feedService;
        private readonly ILogger<FeedController> _logger;

        public FeedController(IFeedService feedService, ILogger<FeedController> logger)
        {
            _feedService = feedService;
            _logger = logger;
        }

        [HttpGet("feed")]
        [Authorize]
        public async Task<IActionResult> GetFeed([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetCurrentUserId();
                var feedRequest = new FeedRequestClientModel
                {
                    Page = page,
                    PageSize = pageSize
                };

                var feed = await _feedService.GetPersonalizedFeedAsync(userId, feedRequest);
                return Ok(feed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting feed");
                return StatusCode(500, new { error = "An error occurred while getting feed" });
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