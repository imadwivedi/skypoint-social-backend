namespace SkyPointSocial.Core.ClientModels.User
{
    /// <summary>
    /// Model for updating user profile information
    /// </summary>
    public class UpdateUserClientModel
    {
        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// URL to user's profile picture
        /// </summary>
        public string ProfilePictureUrl { get; set; }
    }
}