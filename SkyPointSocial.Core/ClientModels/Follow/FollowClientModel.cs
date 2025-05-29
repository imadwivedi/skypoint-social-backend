using System;

namespace SkyPointSocial.Core.ClientModels.Follow
{
    /// <summary>
    /// Model for following/unfollowing a user
    /// </summary>
    public class FollowClientModel
    {
        /// <summary>
        /// ID of the user to follow/unfollow
        /// </summary>
        public Guid UserId { get; set; }
    }
}