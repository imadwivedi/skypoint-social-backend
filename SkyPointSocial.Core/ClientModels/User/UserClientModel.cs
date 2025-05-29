using System;

namespace SkyPointSocial.Core.ClientModels.User
{
    /// <summary>
    /// Represents user information exposed to clients
    /// </summary>
    public class UserClientModel
    {
        /// <summary>
        /// Unique identifier of the user
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Unique username for the user
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User's email address
        /// </summary>
        public string Email { get; set; }

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

        /// <summary>
        /// Account creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Number of followers this user has
        /// </summary>
        public int FollowersCount { get; set; }

        /// <summary>
        /// Number of users this user is following
        /// </summary>
        public int FollowingCount { get; set; }

        /// <summary>
        /// Whether the current authenticated user is following this user
        /// Used for follow/unfollow button state
        /// </summary>
        public bool IsFollowing { get; set; }
    }
}