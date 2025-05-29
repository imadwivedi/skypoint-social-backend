using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Core.ClientModels.Post;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.Core.Interfaces;
using SkyPointSocial.Application.Data;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Service for managing posts and their interactions
    /// </summary>
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly ITimeService _timeService;
        private readonly IVoteService _voteService;
        private readonly IFollowService _followService;

        public PostService(
            AppDbContext context, 
            ITimeService timeService,
            IVoteService voteService,
            IFollowService followService)
        {
            _context = context;
            _timeService = timeService;
            _voteService = voteService;
            _followService = followService;
        }

        /// <summary>
        /// Get a single post by ID
        /// - Includes interactive elements (user vote status)
        /// </summary>
        public async Task<PostClientModel> GetByIdAsync(Guid postId, Guid? currentUserId = null)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Followers)
                .Include(p => p.User)
                    .ThenInclude(u => u.Following)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                return null;

            var clientModel = MapToClientModel(post);

            // Set interactive states if user is authenticated
            if (currentUserId.HasValue)
            {
                clientModel.UserVote = await _voteService.GetUserVoteAsync(currentUserId.Value, postId);
                
                if (post.UserId != currentUserId.Value)
                {
                    clientModel.User.IsFollowing = await _followService.IsFollowingAsync(currentUserId.Value, post.UserId);
                }
            }

            return clientModel;
        }

        /// <summary>
        /// Get all posts by a specific user
        /// - Sorted by most recent first
        /// </summary>
        public async Task<List<PostClientModel>> GetByUserIdAsync(Guid userId, Guid? currentUserId = null, int page = 1, int pageSize = 20)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Followers)
                .Include(p => p.User)
                    .ThenInclude(u => u.Following)
                .Include(p => p.Comments)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var clientModels = new List<PostClientModel>();
            var isFollowing = false;

            // Check follow status once for all posts from the same user
            if (currentUserId.HasValue && currentUserId.Value != userId)
            {
                isFollowing = await _followService.IsFollowingAsync(currentUserId.Value, userId);
            }

            foreach (var post in posts)
            {
                var clientModel = MapToClientModel(post);
                clientModel.User.IsFollowing = isFollowing;

                if (currentUserId.HasValue)
                {
                    clientModel.UserVote = await _voteService.GetUserVoteAsync(currentUserId.Value, post.Id);
                }

                clientModels.Add(clientModel);
            }

            return clientModels;
        }

        /// <summary>
        /// Create a new text-based post
        /// - Content must not be empty
        /// </summary>
        public async Task<PostClientModel> CreateAsync(Guid userId, CreatePostClientModel createPostModel)
        {
            if (string.IsNullOrWhiteSpace(createPostModel.Content))
                throw new ArgumentException("Post content cannot be empty");

            // Use the Post constructor
            var post = new Post(userId, createPostModel.Content);

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Reload with includes for complete data
            post = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Followers)
                .Include(p => p.User)
                    .ThenInclude(u => u.Following)
                .FirstAsync(p => p.Id == post.Id);

            return MapToClientModel(post);
        }

        /// <summary>
        /// Update an existing post
        /// - Only post author can update
        /// </summary>
        public async Task<PostClientModel> UpdateAsync(Guid postId, Guid userId, UpdatePostClientModel updatePostModel)
        {
            if (string.IsNullOrWhiteSpace(updatePostModel.Content))
                throw new ArgumentException("Post content cannot be empty");

            var post = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Followers)
                .Include(p => p.User)
                    .ThenInclude(u => u.Following)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new InvalidOperationException("Post not found");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own posts");

            post.Content = updatePostModel.Content;
            post.UpdatedAt = _timeService.GetCurrentUtcTime();

            await _context.SaveChangesAsync();

            return MapToClientModel(post);
        }

        /// <summary>
        /// Delete a post
        /// - Only post author can delete
        /// - Cascading delete handles related entities
        /// </summary>
        public async Task DeleteAsync(Guid postId, Guid userId)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == postId);

            if (post == null)
                throw new InvalidOperationException("Post not found");

            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own posts");

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get trending posts (high engagement)
        /// - Sorted by score, then comments, then recency
        /// </summary>
        public async Task<List<PostClientModel>> GetTrendingAsync(Guid? currentUserId = null, int page = 1, int pageSize = 20)
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Followers)
                .Include(p => p.User)
                    .ThenInclude(u => u.Following)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.Comments.Count)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var clientModels = new List<PostClientModel>();

            foreach (var post in posts)
            {
                var clientModel = MapToClientModel(post);

                if (currentUserId.HasValue)
                {
                    clientModel.UserVote = await _voteService.GetUserVoteAsync(currentUserId.Value, post.Id);
                    
                    if (post.UserId != currentUserId.Value)
                    {
                        clientModel.User.IsFollowing = await _followService.IsFollowingAsync(currentUserId.Value, post.UserId);
                    }
                }

                clientModels.Add(clientModel);
            }

            return clientModels;
        }

        /// <summary>
        /// Map Post entity to PostClientModel
        /// </summary>
        private PostClientModel MapToClientModel(Post post)
        {
            return new PostClientModel
            {
                Id = post.Id,
                Content = post.Content,
                Score = post.Score,
                CommentCount = post.Comments?.Count ?? 0,
                CreatedAt = post.CreatedAt,
                TimeAgo = _timeService.GetTimeAgo(post.CreatedAt),
                User = new UserClientModel
                {
                    Id = post.User.Id,
                    Username = post.User.Username,
                    Email = post.User.Email,
                    FirstName = post.User.FirstName,
                    LastName = post.User.LastName,
                    ProfilePictureUrl = post.User.ProfilePictureUrl,
                    CreatedAt = post.User.CreatedAt,
                    FollowersCount = post.User.Followers?.Count ?? 0,
                    FollowingCount = post.User.Following?.Count ?? 0,
                    IsFollowing = false // Set by calling method
                },
                UserVote = null // Set by calling method
            };
        }
    }
}