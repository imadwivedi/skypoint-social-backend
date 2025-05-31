using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Application.Data;
using SkyPointSocial.Core.ClientModels.Auth;
using SkyPointSocial.Core.ClientModels.Session;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.Interfaces;

namespace SkyPointSocial.API.Controllers
{
    [ApiController]
    [Route("api")]
    [ProducesResponseType(typeof(AuthResponseClientModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ISessionService _sessionService;
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;

        public AuthController(
            IAuthService authService,
            ISessionService sessionService,
            ILogger<AuthController> logger,
            AppDbContext appDbContext)
        {
            _authService = authService;
            _sessionService = sessionService;
            _logger = logger;
            this._context = appDbContext;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] CreateUserClientModel model)
        {
            try
            {
                var result = await _authService.RegisterAsync(model);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during signup");
                return StatusCode(500, new { error = "An error occurred during signup" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginClientModel model)
        {
            try
            {
                var result = await _authService.LoginAsync(model);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { error = "An error occurred during login" });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(typeof(SessionSummaryClientModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var sessionId = GetSessionId();
                var sessionSummary = await _sessionService.EndSessionAsync(sessionId);

                return Ok(new
                {
                    message = "Logged out successfully",
                    sessionDuration = sessionSummary.Duration
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { error = "An error occurred during logout" });
            }
        }

        [HttpPost("oauth/login")]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginClientModel model)
        {
            try
            {
                var result = await _authService.OAuthLoginAsync(model);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (NotImplementedException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OAuth login");
                return StatusCode(500, new { error = "An error occurred during OAuth login" });
            }
        }

        [HttpGet("users/{userId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetUser(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.Username,
                        u.FirstName,
                        u.LastName,
                        u.ProfilePictureUrl,
                        u.CreatedAt,
                        u.UpdatedAt,
                        PostCount = u.Posts.Count,
                        FollowerCount = u.Followers.Count,
                        FollowingCount = u.Following.Count
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new { error = "User not found" });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user {userId}");
                return StatusCode(500, new { error = "An error occurred while retrieving user", details = ex.Message });
            }
        }

        private Guid GetSessionId()
        {
            var sessionClaim = User.FindFirst("SessionId")?.Value;
            if (string.IsNullOrEmpty(sessionClaim))
            {
                throw new UnauthorizedAccessException("Session ID not found");
            }
            return Guid.Parse(sessionClaim);
        }
    }
}