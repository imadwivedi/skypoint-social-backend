using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Core.ClientModels.Comment;
using SkyPointSocial.Core.ClientModels.User;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.Core.Interfaces;
using SkyPointSocial.Application.Data;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Service for managing comments and nested replies
    /// </summary>
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly ITimeService _timeService;

        public CommentService(AppDbContext context, ITimeService timeService)
        {
            _context = context;
            _timeService = timeService;
        }

        /// <summary>
        /// Get all comments for a post
        /// - Includes nested replies in hierarchical structure
        /// - Includes author information for each comment
        /// - Sorted by creation time
        /// </summary>
        public async Task<List<CommentClientModel>> GetByPostIdAsync(Guid postId)
        {
            // Get all comments for the post with navigation properties
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Where(c => c.PostId == postId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            // Build hierarchical structure - only return top-level comments
            var topLevelComments = comments
                .Where(c => c.ParentCommentId == null)
                .Select(c => MapToClientModel(c, comments))
                .ToList();

            return topLevelComments;
        }

        /// <summary>
        /// Get a single comment by ID
        /// - Includes replies
        /// </summary>
        public async Task<CommentClientModel> GetByIdAsync(Guid commentId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                return null;

            // Get all comments for the post to build the full nested structure
            var allComments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == comment.PostId)
                .ToListAsync();

            return MapToClientModel(comment, allComments);
        }

        /// <summary>
        /// Create a new comment on a post
        /// - Supports both top-level comments and nested replies
        /// - Updates post comment count
        /// </summary>
        public async Task<CommentClientModel> CreateAsync(Guid userId, CreateCommentClientModel createCommentModel)
        {
            if (string.IsNullOrWhiteSpace(createCommentModel.Content))
                throw new ArgumentException("Comment content cannot be empty");

            // Verify post exists
            var postExists = await _context.Posts.AnyAsync(p => p.Id == createCommentModel.PostId);
            if (!postExists)
                throw new InvalidOperationException("Post not found");

            // If it's a reply, verify parent comment exists and belongs to the same post
            if (createCommentModel.ParentCommentId.HasValue)
            {
                var parentExists = await _context.Comments
                    .AnyAsync(c => c.Id == createCommentModel.ParentCommentId.Value && 
                                  c.PostId == createCommentModel.PostId);
                
                if (!parentExists)
                    throw new InvalidOperationException("Parent comment not found or belongs to different post");
            }

            Comment comment;
            if (createCommentModel.ParentCommentId.HasValue)
            {
                // Create as reply using the constructor with parent comment
                comment = new Comment(
                    userId,
                    createCommentModel.PostId,
                    createCommentModel.ParentCommentId.Value,
                    createCommentModel.Content);
            }
            else
            {
                // Create as top-level comment
                comment = new Comment(
                    userId,
                    createCommentModel.PostId,
                    createCommentModel.Content);
            }

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Reload with includes to return full model
            comment = await _context.Comments
                .Include(c => c.User)
                .FirstAsync(c => c.Id == comment.Id);

            // Return without nested replies for newly created comment
            return MapToClientModel(comment, new List<Comment>());
        }

        /// <summary>
        /// Update an existing comment
        /// - Only comment author can update
        /// </summary>
        public async Task<CommentClientModel> UpdateAsync(Guid commentId, Guid userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Comment content cannot be empty");

            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new InvalidOperationException("Comment not found");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own comments");

            comment.Content = content;
            // Note: The Comment entity doesn't have UpdatedAt property based on the context

            await _context.SaveChangesAsync();

            return MapToClientModel(comment, new List<Comment>());
        }

        /// <summary>
        /// Delete a comment
        /// - Only comment author can delete
        /// - Handles nested replies appropriately
        /// - Updates post comment count
        /// </summary>
        public async Task DeleteAsync(Guid commentId, Guid userId)
        {
            var comment = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new InvalidOperationException("Comment not found");

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException("You can only delete your own comments");

            // Check if comment has replies
            if (comment.Replies != null && comment.Replies.Any())
            {
                // Prevent deletion if there are replies
                throw new InvalidOperationException("Cannot delete comment with replies");
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get comment count for a post
        /// </summary>
        public async Task<int> GetCommentCountAsync(Guid postId)
        {
            return await _context.Comments
                .CountAsync(c => c.PostId == postId);
        }

        private CommentClientModel MapToClientModel(Comment comment, List<Comment> allComments)
        {
            var clientModel = new CommentClientModel
            {
                Id = comment.Id,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                TimeAgo = _timeService.GetTimeAgo(comment.CreatedAt),
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                User = new UserClientModel
                {
                    Id = comment.User.Id,
                    Username = comment.User.Username,
                    Email = comment.User.Email,
                    FirstName = comment.User.FirstName,
                    LastName = comment.User.LastName,
                    ProfilePictureUrl = comment.User.ProfilePictureUrl,
                    CreatedAt = comment.User.CreatedAt,
                    FollowersCount = 0, // Not loaded in this context
                    FollowingCount = 0, // Not loaded in this context
                    IsFollowing = false // Not relevant in comment context
                },
                Replies = new List<CommentClientModel>()
            };

            // Build nested replies from the provided comment list
            var directReplies = allComments
                .Where(c => c.ParentCommentId == comment.Id)
                .OrderBy(c => c.CreatedAt);

            foreach (var reply in directReplies)
            {
                clientModel.Replies.Add(MapToClientModel(reply, allComments));
            }

            return clientModel;
        }
    }
}