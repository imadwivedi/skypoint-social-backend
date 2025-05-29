using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyPointSocial.Core.ClientModels.Vote;
using SkyPointSocial.Core.Interfaces;
using System.Security.Claims;

namespace SkyPointSocial.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class VoteController : ControllerBase
    {
        private readonly IVoteService _voteService;
        private readonly ILogger<VoteController> _logger;

        public VoteController(IVoteService voteService, ILogger<VoteController> logger)
        {
            _voteService = voteService;
            _logger = logger;
        }

        [HttpPost("vote")]
        [Authorize]
        public async Task<IActionResult> Vote([FromBody] CreateVoteClientModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _voteService.VoteAsync(userId, model);
                return Ok(new { message = "Vote recorded successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voting");
                return StatusCode(500, new { error = "An error occurred while voting" });
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