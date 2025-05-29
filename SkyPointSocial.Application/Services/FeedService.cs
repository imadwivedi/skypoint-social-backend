using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Core.ClientModels.Feed;
using SkyPointSocial.Core.ClientModels.Post;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.Interfaces;
using SkyPointSocial.Application.Data;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Service for generating personalized news feeds
    /// </summary>
    public class FeedService : IFeedService
    {
        private readonly AppDbContext _context;
        private readonly IFollowService _followService;
        private readonly IVoteService _voteService;
        private readonly ITimeService _timeService;

        public FeedService(
            AppDbContext context, 
            IFollowService followService,
            IVoteService voteService,
            ITimeService timeService)
        {
            _context = context;
            _followService = followService;
            _voteService = voteService;
            _timeService = timeService;
        }

        /// <summary>
        /// Get personalized feed for a user
        /// - Sorts by: 1) Followed users first, 2) Post score, 3) Comment count, 4) Recency
        /// - Includes all interactive controls states
        /// </summary>
        public async Task<FeedResponseClientModel> GetPersonalizedFeedAsync(Guid userId, FeedRequestClientModel feedRequest)
        {
            // Get list of users that the current user is following
            var followingIds = await _followService.GetFollowingIdsAsync(userId);

            // Get all posts with necessary includes
            var query = _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Followers)
                .Include(p => p.User)
                    .ThenInclude(u => u.Following)
                .Include(p => p.Comments)
                .AsQueryable();

            // Calculate total count before pagination
            var totalCount = await query.CountAsync();

            // Apply the sorting algorithm exactly as required
            var posts = await query
                .Select(p => new
                {
                    Post = p,
                    IsFromFollowedUser = followingIds.Contains(p.UserId),
                    CommentCount = p.Comments.Count
                })
                .OrderByDescending(x => x.IsFromFollowedUser ? 1 : 0)  // Priority 1: Followed users first
                .ThenByDescending(x => x.Post.Score)                    // Priority 2: Higher score
                .ThenByDescending(x => x.CommentCount)                  // Priority 3: More comments
                .ThenByDescending(x => x.Post.CreatedAt)               // Priority 4: Most recent
                .Skip((feedRequest.Page - 1) * feedRequest.PageSize)
                .Take(feedRequest.PageSize)
                .ToListAsync();

            // Map to client models
            var postClientModels = new List<PostClientModel>();
            foreach (var item in posts)
            {
                var postModel = new PostClientModel
                {
                    Id = item.Post.Id,
                    Content = item.Post.Content,
                    Score = item.Post.Score,
                    CommentCount = item.CommentCount,
                    CreatedAt = item.Post.CreatedAt,
                    TimeAgo = _timeService.GetTimeAgo(item.Post.CreatedAt),
                    User = new UserClientModel
                    {
                        Id = item.Post.User.Id,
                        Username = item.Post.User.Username,
                        Email = item.Post.User.Email,
                        FirstName = item.Post.User.FirstName,
                        LastName = item.Post.User.LastName,
                        ProfilePictureUrl = item.Post.User.ProfilePictureUrl,
                        CreatedAt = item.Post.User.CreatedAt,
                        FollowersCount = item.Post.User.Followers?.Count ?? 0,
                        FollowingCount = item.Post.User.Following?.Count ?? 0,
                        IsFollowing = item.IsFromFollowedUser
                    }
                };

                // Set user's vote status
                postModel.UserVote = await _voteService.GetUserVoteAsync(userId, item.Post.Id);

                postClientModels.Add(postModel);
            }

            // Calculate if there are more posts available
            var hasMore = (feedRequest.Page * feedRequest.PageSize) < totalCount;

            return new FeedResponseClientModel
            {
                Posts = postClientModels,
                TotalCount = totalCount,
                Page = feedRequest.Page,
                PageSize = feedRequest.PageSize,
                HasMore = hasMore
            };
        }

        /// <summary>
        /// Get public feed (for non-authenticated users)
        /// - Shows trending and recent posts
        /// - No personalization
        /// </summary>
        public async Task<FeedResponseClientModel> GetPublicFeedAsync(FeedRequestClientModel feedRequest)
        {
            var query = _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Followers)
                .Include(p => p.User)
                    .ThenInclude(u => u.Following)
                .Include(p => p.Comments)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            // For public feed, prioritize by engagement and recency
            var posts = await query
                .OrderByDescending(p => p.Score)
                .ThenByDescending(p => p.Comments.Count)
                .ThenByDescending(p => p.CreatedAt)
                .Skip((feedRequest.Page - 1) * feedRequest.PageSize)
                .Take(feedRequest.PageSize)
                .ToListAsync();

            var postClientModels = posts.Select(post => new PostClientModel
            {
                Id = post.Id,
                Content = post.Content,
                Score = post.Score,
                CommentCount = post.Comments.Count,
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
                    FollowersCount = post.User.Followers.Count,
                    FollowingCount = post.User.Following.Count,
                    IsFollowing = false
                },
                UserVote = null // No vote info for public feed
            }).ToList();

            var hasMore = (feedRequest.Page * feedRequest.PageSize) < totalCount;

            return new FeedResponseClientModel
            {
                Posts = postClientModels,
                TotalCount = totalCount,
                Page = feedRequest.Page,
                PageSize = feedRequest.PageSize,
                HasMore = hasMore
            };
        }

        /// <summary>
        /// Get feed of posts from followed users only
        /// - Shows only posts from users being followed
        /// - Sorted by most recent first
        /// </summary>
        public async Task<FeedResponseClientModel> GetFollowingFeedAsync(Guid userId, FeedRequestClientModel feedRequest)
        {
            var followingIds = await _followService.GetFollowingIdsAsync(userId);

            if (!followingIds.Any())
            {
                // Return empty feed if not following anyone
                return new FeedResponseClientModel
                {
                    Posts = new List<PostClientModel>(),
                    TotalCount = 0,
                    Page = feedRequest.Page,
                    PageSize = feedRequest.PageSize,
                    HasMore = false
                };
            }

            var query = _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.Followers)
                .Include(p => p.User)
                    .ThenInclude(u => u.Following)
                .Include(p => p.Comments)
                .Where(p => followingIds.Contains(p.UserId));

            var totalCount = await query.CountAsync();

            var posts = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((feedRequest.Page - 1) * feedRequest.PageSize)
                .Take(feedRequest.PageSize)
                .ToListAsync();

            var postClientModels = new List<PostClientModel>();
            foreach (var post in posts)
            {
                var postModel = new PostClientModel
                {
                    Id = post.Id,
                    Content = post.Content,
                    Score = post.Score,
                    CommentCount = post.Comments.Count,
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
                        FollowersCount = post.User.Followers.Count,
                        FollowingCount = post.User.Following.Count,
                        IsFollowing = true // Always true in following feed
                    }
                };

                postModel.UserVote = await _voteService.GetUserVoteAsync(userId, post.Id);
                postClientModels.Add(postModel);
            }

            var hasMore = (feedRequest.Page * feedRequest.PageSize) < totalCount;

            return new FeedResponseClientModel
            {
                Posts = postClientModels,
                TotalCount = totalCount,
                Page = feedRequest.Page,
                PageSize = feedRequest.PageSize,
                HasMore = hasMore
            };
        }

        /// <summary>
        /// Refresh feed algorithm weights
        /// - Can be used to adjust personalization parameters
        /// </summary>
        public async Task RefreshFeedAlgorithmAsync(Guid userId)
        {
            // This could be used to implement user-specific feed preferences
            // or machine learning-based personalization in the future or user settings
            await Task.CompletedTask;
        }
    }
}