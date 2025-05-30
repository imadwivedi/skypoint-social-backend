using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Core.ClientModels.Vote;
using SkyPointSocial.Core.Entities;
using SkyPointSocial.Core.Interfaces;
using SkyPointSocial.Application.Data;

namespace SkyPointSocial.Application.Services
{
    /// <summary>
    /// Service for managing post voting (upvotes/downvotes)
    /// </summary>
    public class VoteService : IVoteService
    {
        private readonly AppDbContext _context;

        public VoteService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Cast or update a vote on a post
        /// - User can upvote (1) or downvote (-1)
        /// - If user already voted, updates their vote
        /// - Updates post score (upvotes - downvotes)
        /// </summary>
        public async Task VoteAsync(Guid userId, CreateVoteClientModel createVoteModel)
        {
            if (createVoteModel.VoteType != 1 && createVoteModel.VoteType != -1)
            {
                throw new ArgumentException("Vote type must be 1 (upvote) or -1 (downvote).");
            }

            var post = await _context.Posts.FindAsync(createVoteModel.PostId);
            if (post == null)
            {
                throw new InvalidOperationException("Post not found.");
            }

            var existingVote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.PostId == createVoteModel.PostId);

            if (existingVote != null)
            {
                // User has an existing vote on this post
                var oldVoteTypeNumeric = (int)existingVote.Type; // Get the numeric value of the existing vote (e.g., 1 or -1)

                if (oldVoteTypeNumeric == createVoteModel.VoteType)
                {
                    // User is clicking the same vote button again (e.g., upvoting an already upvoted post)
                    // This is an "unvote" action. Remove the existing vote.
                    _context.Votes.Remove(existingVote);
                    post.Score -= oldVoteTypeNumeric; // Revert the score change from the removed vote
                }
                else
                {
                    // User is changing their vote (e.g., from upvote to downvote, or vice-versa)
                    existingVote.Type = createVoteModel.VoteType == 1 ? VoteType.Upvote : VoteType.Downvote;
                    // Adjust score: subtract the old vote's effect, then add the new vote's effect
                    post.Score = post.Score - oldVoteTypeNumeric + createVoteModel.VoteType;
                }
            }
            else
            {
                // No existing vote from this user on this post. Create a new vote.
                var newVoteTypeEnum = createVoteModel.VoteType == 1 ? VoteType.Upvote : VoteType.Downvote;
                var newVote = new Vote(userId, createVoteModel.PostId, newVoteTypeEnum); // Assuming Vote constructor

                _context.Votes.Add(newVote);
                post.Score += createVoteModel.VoteType; // Add the new vote's effect to the score
            }

            post.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

        }

        /// <summary>
        /// Remove user's vote from a post
        /// - Updates post score accordingly
        /// </summary>
        public async Task RemoveVoteAsync(Guid userId, Guid postId)
        {
            var vote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.PostId == postId);

            if (vote == null)
                return; // No vote to remove

            var post = await _context.Posts.FindAsync(postId);
            if (post != null)
            {
                // Update post score
                post.Score -= (int)vote.Type;
                post.UpdatedAt = DateTime.UtcNow;
            }

            _context.Votes.Remove(vote);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get user's vote on a specific post
        /// </summary>
        public async Task<int?> GetUserVoteAsync(Guid userId, Guid postId)
        {
            var vote = await _context.Votes
                .FirstOrDefaultAsync(v => v.UserId == userId && v.PostId == postId);

            return vote != null ? (int)vote.Type : null;
        }

        /// <summary>
        /// Get vote statistics for a post
        /// </summary>
        public async Task<(int upvotes, int downvotes, int score)> GetVoteStatsAsync(Guid postId)
        {
            var votes = await _context.Votes
                .Where(v => v.PostId == postId)
                .ToListAsync();

            var upvotes = votes.Count(v => v.Type == VoteType.Upvote);
            var downvotes = votes.Count(v => v.Type == VoteType.Downvote);
            var score = upvotes - downvotes;

            return (upvotes, downvotes, score);
        }

        /// <summary>
        /// Check if user has voted on a post
        /// </summary>
        public async Task<bool> HasUserVotedAsync(Guid userId, Guid postId)
        {
            return await _context.Votes
                .AnyAsync(v => v.UserId == userId && v.PostId == postId);
        }
    }
}