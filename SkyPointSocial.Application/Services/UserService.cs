using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.Interfaces;
using SkyPointSocial.Application.Data;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Service for managing user profiles and information
    /// </summary>
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IFollowService _followService;

        public UserService(AppDbContext context, IFollowService followService)
        {
            _context = context;
            _followService = followService;
        }

        /// <summary>
        /// Get user by their unique identifier
        /// - Includes follower/following counts
        /// - Checks if current user follows this user
        /// </summary>
        public async Task<UserClientModel> GetByIdAsync(Guid userId, Guid? currentUserId = null)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var clientModel = MapToClientModel(user);

            // Set follow status if current user is provided and not viewing own profile
            if (currentUserId.HasValue && currentUserId.Value != userId)
            {
                clientModel.IsFollowing = await _followService.IsFollowingAsync(currentUserId.Value, userId);
            }

            return clientModel;
        }

        /// <summary>
        /// Get user by email address
        /// </summary>
        public async Task<UserClientModel> GetByEmailAsync(string email)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Email == email);

            return user != null ? MapToClientModel(user) : null;
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public async Task<UserClientModel> GetByUsernameAsync(string username)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Username == username);

            return user != null ? MapToClientModel(user) : null;
        }

        /// <summary>
        /// Search users by username or name
        /// </summary>
        public async Task<List<UserClientModel>> SearchUsersAsync(string searchTerm, Guid? currentUserId = null, int page = 1, int pageSize = 20)
        {
            var query = _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Where(u => u.Username.Contains(searchTerm) || 
                           u.FirstName.Contains(searchTerm) || 
                           u.LastName.Contains(searchTerm));

            var users = await query
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var clientModels = users.Select(u => MapToClientModel(u)).ToList();

            // Set follow status for each user if current user is provided
            if (currentUserId.HasValue)
            {
                foreach (var model in clientModels)
                {
                    if (model.Id != currentUserId.Value)
                    {
                        model.IsFollowing = await _followService.IsFollowingAsync(currentUserId.Value, model.Id);
                    }
                }
            }

            return clientModels;
        }

        /// <summary>
        /// Update user profile information
        /// - Only allows updating own profile
        /// </summary>
        public async Task<UserClientModel> UpdateAsync(Guid userId, UpdateUserClientModel updateUserModel)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new InvalidOperationException("User not found");

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateUserModel.FirstName))
                user.FirstName = updateUserModel.FirstName;
            
            if (!string.IsNullOrEmpty(updateUserModel.LastName))
                user.LastName = updateUserModel.LastName;
            
            if (!string.IsNullOrEmpty(updateUserModel.ProfilePictureUrl))
                user.ProfilePictureUrl = updateUserModel.ProfilePictureUrl;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToClientModel(user);
        }

        /// <summary>
        /// Delete user account
        /// - Soft delete or handle related data appropriately
        /// </summary>
        public async Task DeleteAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
                throw new InvalidOperationException("User not found");

            // Option 1: Hard delete (cascading delete should handle related entities)
            _context.Users.Remove(user);
            
            // Option 2: Soft delete (would require adding IsDeleted property to User entity)
            // user.IsDeleted = true;
            // user.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get user statistics (posts count, followers, following)
        /// </summary>
        public async Task<UserClientModel> GetUserStatsAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return null;

            var clientModel = MapToClientModel(user);
            
            // Posts count could be included if needed
            // clientModel.PostsCount = user.Posts?.Count ?? 0;
            
            return clientModel;
        }

        /// <summary>
        /// Map User entity to UserClientModel
        /// </summary>
        private UserClientModel MapToClientModel(Core.Entities.User user)
        {
            return new UserClientModel
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                CreatedAt = user.CreatedAt,
                FollowersCount = user.Followers?.Count ?? 0,
                FollowingCount = user.Following?.Count ?? 0,
                IsFollowing = false // Default value, set by calling method when needed
            };
        }
    }
}