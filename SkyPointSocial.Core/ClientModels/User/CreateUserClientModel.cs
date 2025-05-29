namespace SkyPointSocial.Core.ClientModels.User
{
    /// <summary>
    /// Model for creating a new user account via email signup
    /// </summary>
    public class CreateUserClientModel
    {
        /// <summary>
        /// Desired username (must be unique)
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Email address (must be unique, prevents duplicate accounts)
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password for authentication
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }
    }
}