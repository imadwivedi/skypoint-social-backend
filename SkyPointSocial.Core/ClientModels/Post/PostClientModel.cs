using System;
using SkyPointSocial.Core.ClientModels.User;

namespace SkyPointSocial.Core.ClientModels.Post
{
    /// <summary>
    /// Represents a post in the feed
    /// </summary>
    public class PostClientModel
    {
        /// <summary>
        /// Unique identifier of the post
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Text content of the post
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Current score (upvotes - downvotes)
        /// Used for feed sorting by popularity
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Total number of comments on this post
        /// Displayed in feed as engagement metric
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// Post creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Human-readable time since posting (e.g., "2h ago")
        /// </summary>
        public string TimeAgo { get; set; }

        /// <summary>
        /// Author information
        /// </summary>
        public UserClientModel User { get; set; }

        /// <summary>
        /// Current user's vote on this post
        /// 1 for upvote, -1 for downvote, null for no vote
        /// Used to show vote button state
        /// </summary>
        public int? UserVote { get; set; }
    }
}