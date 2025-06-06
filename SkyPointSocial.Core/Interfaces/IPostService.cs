using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyPointSocial.Core.ClientModels.Post;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for managing posts and their interactions
    /// </summary>
    public interface IPostService
    {
        /// <summary>
        /// Get a single post by ID
        /// - Includes author information
        /// - Includes vote count and comment count
        /// - Includes current user's vote status
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="currentUserId">Current user ID to check vote status</param>
        /// <returns>Post with all details</returns>
        Task<PostClientModel> GetByIdAsync(Guid postId, Guid? currentUserId = null);

        /// <summary>
        /// Get all posts by a specific user
        /// - Sorted by most recent first
        /// - Includes engagement metrics
        /// </summary>
        /// <param name="userId">Author's user ID</param>
        /// <param name="currentUserId">Current user ID for vote status</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>List of user's posts</returns>
        Task<List<PostClientModel>> GetByUserIdAsync(Guid userId, Guid? currentUserId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Create a new text-based post
        /// </summary>
        /// <param name="userId">Author's user ID</param>
        /// <param name="createPostModel">Post content</param>
        /// <returns>Created post with initial metrics</returns>
        Task<PostClientModel> CreateAsync(Guid userId, CreatePostClientModel createPostModel);

        /// <summary>
        /// Update an existing post
        /// - Only post author can update
        /// </summary>
        /// <param name="postId">Post ID to update</param>
        /// <param name="userId">User ID (must be post author)</param>
        /// <param name="updatePostModel">Updated content</param>
        /// <returns>Updated post</returns>
        Task<PostClientModel> UpdateAsync(Guid postId, Guid userId, UpdatePostClientModel updatePostModel);

        /// <summary>
        /// Delete a post
        /// - Only post author can delete
        /// - Should handle cascading for votes/comments
        /// </summary>
        /// <param name="postId">Post ID to delete</param>
        /// <param name="userId">User ID (must be post author)</param>
        Task DeleteAsync(Guid postId, Guid userId);

        /// <summary>
        /// Get trending posts (high engagement)
        /// - Sorted by score and comment count
        /// </summary>
        /// <param name="currentUserId">Current user ID for vote status</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>List of trending posts</returns>
        Task<List<PostClientModel>> GetTrendingAsync(Guid? currentUserId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Search posts with flexible filtering and extensible criteria
        /// - Supports text search in post content
        /// - Extensible for future search enhancements
        /// - Returns optimized search results with pagination
        /// </summary>
        /// <param name="searchRequest">Search parameters and filters</param>
        /// <param name="currentUserId">Current user ID for personalization</param>
        /// <returns>Paginated search results</returns>
        Task<PostSearchResultClientModel> SearchAsync(PostSearchRequestClientModel searchRequest, Guid? currentUserId = null);
    }
}