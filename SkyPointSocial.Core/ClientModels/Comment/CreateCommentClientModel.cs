using System;

namespace SkyPointSocial.Core.ClientModels.Comment
{
    /// <summary>
    /// Model for creating a new comment
    /// </summary>
    public class CreateCommentClientModel
    {
        /// <summary>
        /// Text content of the comment
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// ID of the post being commented on
        /// </summary>
        public Guid PostId { get; set; }

        /// <summary>
        /// ID of parent comment for nested replies
        /// Null for top-level comments
        /// </summary>
        public Guid? ParentCommentId { get; set; }
    }
}