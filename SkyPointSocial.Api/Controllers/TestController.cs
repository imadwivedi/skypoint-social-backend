using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Application.Data;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.Entities;

namespace SkyPointSocial.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(AppDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    // Try to ensure database is created
                    await _context.Database.EnsureCreatedAsync();
                    
                    // Get table info
                    var tables = await _context.Database.ExecuteSqlRawAsync(@"
                        SELECT table_name 
                        FROM information_schema.tables 
                        WHERE table_schema = 'public'
                    ");
                }
                
                return Ok(new
                {
                    canConnect,
                    connectionString = _context.Database.GetConnectionString()?.Split(';')[0], // Show only host part
                    provider = _context.Database.ProviderName,
                    databaseExists = canConnect
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserClientModel model)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Username);
                
                if (existingUser != null)
                {
                    return BadRequest(new { error = "User with this email or username already exists" });
                }

                // Create new user
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = model.Email,
                    Username = model.Username,
                    PasswordHash = "hashed_" + model.Password, // Simple hash for testing
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created user: {user.Username} with ID: {user.Id}");

                return Ok(new
                {
                    message = "User created successfully",
                    userId = user.Id.ToString(), // Convert to string to avoid serialization issues
                    username = user.Username,
                    email = user.Email
                });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error creating user");
                return StatusCode(500, new 
                { 
                    error = "Database error while creating user", 
                    details = dbEx.Message,
                    innerException = dbEx.InnerException?.Message,
                    data = dbEx.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new 
                { 
                    error = "An error occurred while creating user", 
                    details = ex.Message,
                    innerException = ex.InnerException?.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("get-user/{userId:guid}")]
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
    }
}