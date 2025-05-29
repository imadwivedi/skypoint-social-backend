using System;
using System.Threading.Tasks;
using SkyPointSocial.Core.ClientModels.Feed;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for generating personalized news feeds
    /// </summary>
    public interface IFeedService
    {
        /// <summary>
        /// Get personalized feed for a user
        /// - Sorts by: 1) Followed users first, 2) Post score, 3) Comment count, 4) Recency
        /// - Includes all interactive controls states
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="feedRequest">Pagination parameters</param>
        /// <returns>Paginated personalized feed</returns>
        Task<FeedResponseClientModel> GetPersonalizedFeedAsync(Guid userId, FeedRequestClientModel feedRequest);

        /// <summary>
        /// Get public feed (for non-authenticated users)
        /// - Shows trending and recent posts
        /// - No personalization
        /// </summary>
        /// <param name="feedRequest">Pagination parameters</param>
        /// <returns>Paginated public feed</returns>
        Task<FeedResponseClientModel> GetPublicFeedAsync(FeedRequestClientModel feedRequest);

        /// <summary>
        /// Get feed of posts from followed users only
        /// - Shows only posts from users being followed
        /// - Sorted by most recent first
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="feedRequest">Pagination parameters</param>
        /// <returns>Paginated feed with posts from followed users</returns>
        Task<FeedResponseClientModel> GetFollowingFeedAsync(Guid userId, FeedRequestClientModel feedRequest);

        /// <summary>
        /// Refresh feed algorithm weights
        /// - Can be used to adjust personalization parameters
        /// </summary>
        /// <param name="userId">User ID</param>
        Task RefreshFeedAlgorithmAsync(Guid userId);
    }
}