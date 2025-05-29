namespace SkyPointSocial.Core.ClientModels.Auth
{
    /// <summary>
    /// Model for email-based login
    /// </summary>
    public class LoginClientModel
    {
        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// User's password
        /// </summary>
        public string Password { get; set; }
    }
}