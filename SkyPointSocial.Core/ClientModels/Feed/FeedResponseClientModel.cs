using System.Collections.Generic;
using SkyPointSocial.Core.ClientModels.Post;

namespace SkyPointSocial.Core.ClientModels.Feed
{
    /// <summary>
    /// Response model for personalized feed
    /// </summary>
    public class FeedResponseClientModel
    {
        /// <summary>
        /// List of posts in the feed, sorted by:
        /// 1. Posts by followed users (higher priority)
        /// 2. Post score (upvotes - downvotes)
        /// 3. Number of comments
        /// 4. Most recent first
        /// </summary>
        public List<PostClientModel> Posts { get; set; }

        /// <summary>
        /// Total number of posts available
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of posts per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Whether more posts are available
        /// </summary>
        public bool HasMore { get; set; }
    }
}