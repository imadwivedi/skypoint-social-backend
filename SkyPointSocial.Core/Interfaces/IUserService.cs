using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SkyPointSocial.Core.ClientModels.User;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for managing user profiles and information
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Get user by their unique identifier
        /// - Includes follower/following counts
        /// - Checks if current user follows this user
        /// </summary>
        /// <param name="userId">User ID to retrieve</param>
        /// <param name="currentUserId">Current authenticated user ID for follow status</param>
        /// <returns>User information with social metrics</returns>
        Task<UserClientModel> GetByIdAsync(Guid userId, Guid? currentUserId = null);

        /// <summary>
        /// Get user by email address
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>User information</returns>
        Task<UserClientModel> GetByEmailAsync(string email);

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>User information</returns>
        Task<UserClientModel> GetByUsernameAsync(string username);

        /// <summary>
        /// Search users by username or name
        /// </summary>
        /// <param name="searchTerm">Search term</param>
        /// <param name="currentUserId">Current user ID for follow status</param>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Items per page</param>
        /// <returns>List of matching users</returns>
        Task<List<UserClientModel>> SearchUsersAsync(string searchTerm, Guid? currentUserId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Update user profile information
        /// - Only allows updating own profile
        /// </summary>
        /// <param name="userId">User ID to update</param>
        /// <param name="updateUserModel">Updated profile information</param>
        /// <returns>Updated user information</returns>
        Task<UserClientModel> UpdateAsync(Guid userId, UpdateUserClientModel updateUserModel);

        /// <summary>
        /// Delete user account
        /// - Soft delete or handle related data appropriately
        /// </summary>
        /// <param name="userId">User ID to delete</param>
        Task DeleteAsync(Guid userId);

        /// <summary>
        /// Get user statistics (posts count, followers, following)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>User with updated statistics</returns>
        Task<UserClientModel> GetUserStatsAsync(Guid userId);
    }
}