using System;

namespace SkyPointSocial.Core.ClientModels.Post
{
    /// <summary>
    /// Simplified user information for post search results
    /// Excludes follower/following data to reduce payload size
    /// </summary>
    public class PostSearchUserClientModel
    {
        /// <summary>
        /// Unique identifier of the user
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Unique username for the user
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// URL to user's profile picture
        /// </summary>
        public string? ProfilePictureUrl { get; set; }
    }
}
