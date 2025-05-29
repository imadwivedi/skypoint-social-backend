using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyPointSocial.Core.ClientModels.User;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for managing user follow relationships
    /// </summary>
    public interface IFollowService
    {
        /// <summary>
        /// Follow another user
        /// - Creates follow relationship
        /// - Cannot follow yourself
        /// - Cannot follow same user twice
        /// - Followed users' content appears with higher priority in feed
        /// </summary>
        /// <param name="followerId">User who is following</param>
        /// <param name="followingId">User being followed</param>
        Task FollowAsync(Guid followerId, Guid followingId);

        /// <summary>
        /// Unfollow a user
        /// - Removes follow relationship
        /// - Affects feed prioritization
        /// </summary>
        /// <param name="followerId">User who is unfollowing</param>
        /// <param name="followingId">User being unfollowed</param>
        Task UnfollowAsync(Guid followerId, Guid followingId);

        /// <summary>
        /// Get list of users following a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="currentUserId">Current user ID to check follow status</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>List of followers</returns>
        Task<List<UserClientModel>> GetFollowersAsync(Guid userId, Guid? currentUserId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get list of users that a specific user is following
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="currentUserId">Current user ID to check follow status</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>List of followed users</returns>
        Task<List<UserClientModel>> GetFollowingAsync(Guid userId, Guid? currentUserId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Check if one user follows another
        /// </summary>
        /// <param name="followerId">Potential follower ID</param>
        /// <param name="followingId">Potentially followed user ID</param>
        /// <returns>True if following relationship exists</returns>
        Task<bool> IsFollowingAsync(Guid followerId, Guid followingId);

        /// <summary>
        /// Get follower and following counts for a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Follower count and following count</returns>
        Task<(int followers, int following)> GetFollowCountsAsync(Guid userId);

        /// <summary>
        /// Get list of user IDs that a user is following
        /// - Used for feed personalization
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>List of followed user IDs</returns>
        Task<List<Guid>> GetFollowingIdsAsync(Guid userId);
    }
}