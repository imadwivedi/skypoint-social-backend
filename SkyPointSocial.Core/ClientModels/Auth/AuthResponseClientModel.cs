using SkyPointSocial.Core.ClientModels.User;

namespace SkyPointSocial.Core.ClientModels.Auth
{
    /// <summary>
    /// Response model for successful authentication
    /// </summary>
    public class AuthResponseClientModel
    {
        /// <summary>
        /// JWT token for authenticated API requests
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Authenticated user's information
        /// </summary>
        public UserClientModel User { get; set; }

        /// <summary>
        /// Session identifier for tracking login session
        /// </summary>
        public Guid SessionId { get; set; }
    }
}