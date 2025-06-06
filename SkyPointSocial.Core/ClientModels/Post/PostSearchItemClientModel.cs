using System;

namespace SkyPointSocial.Core.ClientModels.Post
{
    /// <summary>
    /// Represents a post in search results with optimized data structure
    /// </summary>
    public class PostSearchItemClientModel
    {
        /// <summary>
        /// Unique identifier of the post
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Text content of the post
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Current score (upvotes - downvotes)
        /// Used for relevance ranking
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Total number of comments on this post
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// Post creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Human-readable time since posting (e.g., "2h ago")
        /// </summary>
        public string TimeAgo { get; set; } = string.Empty;

        /// <summary>
        /// Simplified author information (excludes follower/following data)
        /// </summary>
        public PostSearchUserClientModel User { get; set; } = new();
    }
}
