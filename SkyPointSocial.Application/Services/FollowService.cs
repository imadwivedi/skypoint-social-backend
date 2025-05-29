using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.Core.Interfaces;
using SkyPointSocial.Application.Data;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Service for managing user follow relationships
    /// </summary>
    public class FollowService : IFollowService
    {
        private readonly AppDbContext _context;

        public FollowService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Follow another user
        /// - Creates follow relationship
        /// - Cannot follow yourself
        /// - Cannot follow same user twice
        /// - Followed users' content appears with higher priority in feed
        /// </summary>
        public async Task FollowAsync(Guid followerId, Guid followingId)
        {
            // Cannot follow yourself
            if (followerId == followingId)
                throw new InvalidOperationException("Cannot follow yourself");

            // Check if users exist
            var followerExists = await _context.Users.AnyAsync(u => u.Id == followerId);
            var followingExists = await _context.Users.AnyAsync(u => u.Id == followingId);

            if (!followerExists || !followingExists)
                throw new InvalidOperationException("User not found");

            // Check if already following
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (existingFollow != null)
                return; // Already following

            // Create follow relationship
            var follow = new Follow
            {
                Id = Guid.NewGuid(),
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Unfollow a user
        /// - Removes follow relationship
        /// - Affects feed prioritization
        /// </summary>
        public async Task UnfollowAsync(Guid followerId, Guid followingId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (follow == null)
                return; // Not following

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get list of users following a specific user
        /// </summary>
        public async Task<List<UserClientModel>> GetFollowersAsync(Guid userId, Guid? currentUserId = null, int page = 1, int pageSize = 20)
        {
            var followers = await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Include(f => f.Follower)
                    .ThenInclude(u => u.Followers)
                .Include(f => f.Follower)
                    .ThenInclude(u => u.Following)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Follower)
                .ToListAsync();

            var clientModels = followers.Select(user => MapToUserClientModel(user)).ToList();

            // Set follow status if current user is provided
            if (currentUserId.HasValue)
            {
                var currentUserFollowing = await GetFollowingIdsAsync(currentUserId.Value);
                foreach (var model in clientModels)
                {
                    model.IsFollowing = currentUserFollowing.Contains(model.Id);
                }
            }

            return clientModels;
        }

        /// <summary>
        /// Get list of users that a specific user is following
        /// </summary>
        public async Task<List<UserClientModel>> GetFollowingAsync(Guid userId, Guid? currentUserId = null, int page = 1, int pageSize = 20)
        {
            var following = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Following)
                    .ThenInclude(u => u.Followers)
                .Include(f => f.Following)
                    .ThenInclude(u => u.Following)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Following)
                .ToListAsync();

            var clientModels = following.Select(user => MapToUserClientModel(user)).ToList();

            // Set follow status if current user is provided
            if (currentUserId.HasValue)
            {
                var currentUserFollowing = await GetFollowingIdsAsync(currentUserId.Value);
                foreach (var model in clientModels)
                {
                    model.IsFollowing = currentUserFollowing.Contains(model.Id);
                }
            }

            return clientModels;
        }

        /// <summary>
        /// Check if one user follows another
        /// </summary>
        public async Task<bool> IsFollowingAsync(Guid followerId, Guid followingId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        /// <summary>
        /// Get follower and following counts for a user
        /// </summary>
        public async Task<(int followers, int following)> GetFollowCountsAsync(Guid userId)
        {
            var followers = await _context.Follows.CountAsync(f => f.FollowingId == userId);
            var following = await _context.Follows.CountAsync(f => f.FollowerId == userId);

            return (followers, following);
        }

        /// <summary>
        /// Get list of user IDs that a user is following
        /// - Used for feed personalization
        /// </summary>
        public async Task<List<Guid>> GetFollowingIdsAsync(Guid userId)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();
        }

        private UserClientModel MapToUserClientModel(User user)
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
                IsFollowing = false // Set by calling method
            };
        }
    }
}