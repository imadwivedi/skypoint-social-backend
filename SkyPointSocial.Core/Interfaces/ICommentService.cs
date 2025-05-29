using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyPointSocial.Core.ClientModels.Comment;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for managing comments and nested replies
    /// </summary>
    public interface ICommentService
    {
        /// <summary>
        /// Get all comments for a post
        /// - Includes nested replies in hierarchical structure
        /// - Includes author information for each comment
        /// - Sorted by creation time
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <returns>List of comments with nested replies</returns>
        Task<List<CommentClientModel>> GetByPostIdAsync(Guid postId);

        /// <summary>
        /// Get a single comment by ID
        /// - Includes replies
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <returns>Comment with details</returns>
        Task<CommentClientModel> GetByIdAsync(Guid commentId);

        /// <summary>
        /// Create a new comment on a post
        /// - Supports both top-level comments and nested replies
        /// - Updates post comment count
        /// </summary>
        /// <param name="userId">Comment author ID</param>
        /// <param name="createCommentModel">Comment details</param>
        /// <returns>Created comment</returns>
        Task<CommentClientModel> CreateAsync(Guid userId, CreateCommentClientModel createCommentModel);

        /// <summary>
        /// Update an existing comment
        /// - Only comment author can update
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <param name="userId">User ID (must be comment author)</param>
        /// <param name="content">Updated content</param>
        /// <returns>Updated comment</returns>
        Task<CommentClientModel> UpdateAsync(Guid commentId, Guid userId, string content);

        /// <summary>
        /// Delete a comment
        /// - Only comment author can delete
        /// - Handles nested replies appropriately
        /// - Updates post comment count
        /// </summary>
        /// <param name="commentId">Comment ID</param>
        /// <param name="userId">User ID (must be comment author)</param>
        Task DeleteAsync(Guid commentId, Guid userId);

        /// <summary>
        /// Get comment count for a post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <returns>Total number of comments including replies</returns>
        Task<int> GetCommentCountAsync(Guid postId);
    }
}