using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SkyPointSocial.Core.ClientModels.Auth;
using SkyPointSocial.Core.ClientModels.Session;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.Core.Interfaces;
using SkyPointSocial.Application.Data;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Service for handling authentication operations
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ISessionService _sessionService;
        private readonly ITimeService _timeService;

        public AuthService(
            AppDbContext context, 
            IConfiguration configuration,
            ISessionService sessionService,
            ITimeService timeService)
        {
            _context = context;
            _configuration = configuration;
            _sessionService = sessionService;
            _timeService = timeService;
        }

        /// <summary>
        /// Register a new user with email and password
        /// </summary>
        public async Task<AuthResponseClientModel> RegisterAsync(CreateUserClientModel createUserModel)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(createUserModel.Email))
                throw new ArgumentException("Email is required");
            
            if (string.IsNullOrWhiteSpace(createUserModel.Password))
                throw new ArgumentException("Password is required");
            
            if (string.IsNullOrWhiteSpace(createUserModel.Username))
                throw new ArgumentException("Username is required");

            // Check for duplicate email
            if (await IsEmailRegisteredAsync(createUserModel.Email))
                throw new InvalidOperationException("Email is already registered");

            // Check for duplicate username
            if (await IsUsernameTakenAsync(createUserModel.Username))
                throw new InvalidOperationException("Username is already taken");

            // Create new user
            var passwordHash = HashPassword(createUserModel.Password);
            
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = createUserModel.Username,
                Email = createUserModel.Email,
                PasswordHash = passwordHash,
                FirstName = createUserModel.FirstName ?? string.Empty,
                LastName = createUserModel.LastName ?? string.Empty,
                ProfilePictureUrl = null,
                OAuthProvider = null,
                OAuthProviderId = null,
                CreatedAt = _timeService.GetCurrentUtcTime(),
                UpdatedAt = _timeService.GetCurrentUtcTime()
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstAsync(u => u.Id == user.Id);

            // Create session
            var sessionId = await _sessionService.CreateSessionAsync(user.Id);

            // Generate token
            var token = GenerateJwtToken(user, sessionId);

            return new AuthResponseClientModel
            {
                Token = token,
                User = MapToUserClientModel(user),
                SessionId = sessionId
            };
        }

        /// <summary>
        /// Authenticate user with email and password
        /// </summary>
        public async Task<AuthResponseClientModel> LoginAsync(LoginClientModel loginModel)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(loginModel.Email))
                throw new ArgumentException("Email is required");
            
            if (string.IsNullOrWhiteSpace(loginModel.Password))
                throw new ArgumentException("Password is required");

            // Find user by email (case-insensitive)
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginModel.Email.ToLower());

            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            // Verify password
            if (!VerifyPassword(loginModel.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            // Create session
            var sessionId = await _sessionService.CreateSessionAsync(user.Id);

            // Generate token
            var token = GenerateJwtToken(user, sessionId);

            return new AuthResponseClientModel
            {
                Token = token,
                User = MapToUserClientModel(user),
                SessionId = sessionId
            };
        }

        /// <summary>
        /// Authenticate user via OAuth provider (Google or Microsoft)
        /// </summary>
        public async Task<AuthResponseClientModel> OAuthLoginAsync(OAuthLoginClientModel oAuthLoginModel)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(oAuthLoginModel.Provider))
                throw new ArgumentException("OAuth provider is required");
            
            if (string.IsNullOrWhiteSpace(oAuthLoginModel.AccessToken))
                throw new ArgumentException("Access token is required");

            // Validate provider
            if (oAuthLoginModel.Provider != "Google" && oAuthLoginModel.Provider != "Microsoft")
                throw new ArgumentException("Only Google and Microsoft OAuth providers are supported");

            // Validate OAuth token with provider
            var oAuthUserInfo = await ValidateOAuthToken(oAuthLoginModel);

            // Find existing user by email
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == oAuthUserInfo.Email.ToLower());

            if (user == null)
            {
                // First-time OAuth login - create user
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = await GenerateUsernameFromEmail(oAuthUserInfo.Email),
                    Email = oAuthUserInfo.Email,
                    PasswordHash = string.Empty, // No password for OAuth users
                    FirstName = oAuthUserInfo.FirstName ?? string.Empty,
                    LastName = oAuthUserInfo.LastName ?? string.Empty,
                    ProfilePictureUrl = oAuthUserInfo.ProfilePictureUrl,
                    OAuthProvider = oAuthLoginModel.Provider,
                    OAuthProviderId = oAuthUserInfo.ProviderId,
                    CreatedAt = _timeService.GetCurrentUtcTime(),
                    UpdatedAt = _timeService.GetCurrentUtcTime()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Reload with navigation properties
                user = await _context.Users
                    .Include(u => u.Followers)
                    .Include(u => u.Following)
                    .FirstAsync(u => u.Id == user.Id);
            }
            else
            {
                // Check if OAuth provider matches
                if (!string.IsNullOrEmpty(user.OAuthProvider) && 
                    (user.OAuthProvider != oAuthLoginModel.Provider || user.OAuthProviderId != oAuthUserInfo.ProviderId))
                {
                    throw new InvalidOperationException("This email is already registered with a different provider");
                }

                // Link OAuth to existing account if not already linked
                if (string.IsNullOrEmpty(user.OAuthProvider))
                {
                    user.OAuthProvider = oAuthLoginModel.Provider;
                    user.OAuthProviderId = oAuthUserInfo.ProviderId;
                    user.UpdatedAt = _timeService.GetCurrentUtcTime();
                    await _context.SaveChangesAsync();
                }
            }

            // Create session
            var sessionId = await _sessionService.CreateSessionAsync(user.Id);

            // Generate token
            var token = GenerateJwtToken(user, sessionId);

            return new AuthResponseClientModel
            {
                Token = token,
                User = MapToUserClientModel(user),
                SessionId = sessionId
            };
        }

        /// <summary>
        /// Logout user and end their session
        /// </summary>
        public async Task<SessionSummaryClientModel> LogoutAsync(Guid sessionId)
        {
            return await _sessionService.EndSessionAsync(sessionId);
        }

        /// <summary>
        /// Validate if an email is already registered
        /// </summary>
        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        /// <summary>
        /// Validate if a username is already taken
        /// </summary>
        public async Task<bool> IsUsernameTakenAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        /// <summary>
        /// Hash password using BCrypt
        /// </summary>
        private string HashPassword(string password)
        {
            // Simple hash for now - in production use BCrypt.Net-Next package
            // Install-Package BCrypt.Net-Next
            // return BCrypt.Net.BCrypt.HashPassword(password);
            
            // Temporary implementation using SHA256
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "SkyPointSalt"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify password against hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            // For BCrypt use: return BCrypt.Net.BCrypt.Verify(password, hash);
            
            // Temporary implementation
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        private string GenerateJwtToken(User user, Guid sessionId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"];
            
            if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
            {
                // Use a default key for development (should be configured properly in production)
                jwtKey = "ThisIsADefaultDevelopmentKeyThatShouldBeChangedInProduction123!";
            }

            var key = Encoding.UTF8.GetBytes(jwtKey);
            
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("firstName", user.FirstName ?? string.Empty),
                    new Claim("lastName", user.LastName ?? string.Empty),
                    new Claim("SessionId", sessionId.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), 
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"] ?? "SkyPointSocial",
                Audience = _configuration["Jwt:Audience"] ?? "SkyPointSocialUsers"
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate OAuth token with provider and get user info
        /// </summary>
        private async Task<OAuthUserInfo> ValidateOAuthToken(OAuthLoginClientModel oAuthLoginModel)
        {
            // TODO: Implement actual OAuth validation
            // This is a placeholder for development
            
            await Task.CompletedTask;
            
            // In production, validate the token with the OAuth provider
            // and return real user information
            
            // For now, return mock data for testing
            var email = $"test_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
            
            return new OAuthUserInfo
            {
                Email = email,
                FirstName = "Test",
                LastName = "User",
                ProviderId = Guid.NewGuid().ToString(),
                ProfilePictureUrl = null
            };
        }

        /// <summary>
        /// Generate unique username from email
        /// </summary>
        private async Task<string> GenerateUsernameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new ArgumentException("Invalid email format");

            var baseUsername = email.Split('@')[0].ToLower();
            
            // Remove special characters
            baseUsername = System.Text.RegularExpressions.Regex.Replace(baseUsername, @"[^a-zA-Z0-9]", "");
            
            if (string.IsNullOrWhiteSpace(baseUsername))
                baseUsername = "user";

            var username = baseUsername;
            var counter = 1;

            // Keep trying until we find a unique username
            while (await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower()))
            {
                username = $"{baseUsername}{counter}";
                counter++;
            }

            return username;
        }

        /// <summary>
        /// Map User entity to UserClientModel
        /// </summary>
        private UserClientModel MapToUserClientModel(User user)
        {
            return new UserClientModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt,
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0,
                IsFollowing = false // Not applicable in auth context
            };
        }
    }
}