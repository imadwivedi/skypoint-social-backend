using System.Threading.Tasks;
using SkyPointSocial.Core.ClientModels.Auth;
using SkyPointSocial.Core.ClientModels.Session;
using SkyPointSocial.Core.ClientModels.User;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for handling authentication operations including email signup/login and OAuth
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Register a new user with email and password
        /// - Accepts any valid email
        /// - Prevents duplicate accounts by checking email uniqueness
        /// - Creates a new session for the user
        /// </summary>
        /// <param name="createUserModel">User registration details</param>
        /// <returns>Authentication response with JWT token and user info</returns>
        Task<AuthResponseClientModel> RegisterAsync(CreateUserClientModel createUserModel);

        /// <summary>
        /// Authenticate user with email and password
        /// - Validates credentials
        /// - Creates a new session for the user
        /// </summary>
        /// <param name="loginModel">Email and password</param>
        /// <returns>Authentication response with JWT token and user info</returns>
        Task<AuthResponseClientModel> LoginAsync(LoginClientModel loginModel);

        /// <summary>
        /// Authenticate user via OAuth provider (Google or Microsoft)
        /// - First-time login auto-creates the user account
        /// - Links OAuth provider info to user account
        /// - Creates a new session for the user
        /// </summary>
        /// <param name="oAuthLoginModel">OAuth provider and access token</param>
        /// <returns>Authentication response with JWT token and user info</returns>
        Task<AuthResponseClientModel> OAuthLoginAsync(OAuthLoginClientModel oAuthLoginModel);

        /// <summary>
        /// Logout user and end their session
        /// - Updates session with logout time
        /// - Calculates and returns session duration
        /// </summary>
        /// <param name="sessionId">Current session identifier</param>
        /// <returns>Session summary with duration</returns>
        Task<SessionSummaryClientModel> LogoutAsync(Guid sessionId);

        /// <summary>
        /// Validate if an email is already registered
        /// </summary>
        /// <param name="email">Email to check</param>
        /// <returns>True if email exists, false otherwise</returns>
        Task<bool> IsEmailRegisteredAsync(string email);

        /// <summary>
        /// Validate if a username is already taken
        /// </summary>
        /// <param name="username">Username to check</param>
        /// <returns>True if username exists, false otherwise</returns>
        Task<bool> IsUsernameTakenAsync(string username);
    }
}