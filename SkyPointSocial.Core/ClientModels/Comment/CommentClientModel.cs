using System;
using System.Collections.Generic;
using SkyPointSocial.Core.ClientModels.User;

namespace SkyPointSocial.Core.ClientModels.Comment
{
    /// <summary>
    /// Represents a comment on a post
    /// </summary>
    public class CommentClientModel
    {
        /// <summary>
        /// Unique identifier of the comment
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Text content of the comment
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Comment creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Human-readable time since commenting (e.g., "2h ago")
        /// </summary>
        public string TimeAgo { get; set; }

        /// <summary>
        /// Comment author information
        /// </summary>
        public UserClientModel User { get; set; }

        /// <summary>
        /// ID of the post this comment belongs to
        /// </summary>
        public Guid PostId { get; set; }

        /// <summary>
        /// ID of parent comment if this is a nested reply
        /// Null for top-level comments
        /// </summary>
        public Guid? ParentCommentId { get; set; }

        /// <summary>
        /// Nested replies to this comment
        /// Supports threaded discussions
        /// </summary>
        public List<CommentClientModel> Replies { get; set; }
    }
}