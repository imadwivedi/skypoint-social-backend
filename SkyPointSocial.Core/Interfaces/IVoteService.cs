using System;
using System.Threading.Tasks;
using SkyPointSocial.Core.ClientModels.Vote;

namespace SkyPointSocial.Core.Interfaces
{
    /// <summary>
    /// Service for managing post voting (upvotes/downvotes)
    /// </summary>
    public interface IVoteService
    {
        /// <summary>
        /// Cast or update a vote on a post
        /// - User can upvote (1) or downvote (-1)
        /// - If user already voted, updates their vote
        /// - Updates post score (upvotes - downvotes)
        /// </summary>
        /// <param name="userId">Voting user ID</param>
        /// <param name="createVoteModel">Vote details</param>
        Task VoteAsync(Guid userId, CreateVoteClientModel createVoteModel);

        /// <summary>
        /// Remove user's vote from a post
        /// - Updates post score accordingly
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="postId">Post ID</param>
        Task RemoveVoteAsync(Guid userId, Guid postId);

        /// <summary>
        /// Get user's vote on a specific post
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="postId">Post ID</param>
        /// <returns>1 for upvote, -1 for downvote, null if no vote</returns>
        Task<int?> GetUserVoteAsync(Guid userId, Guid postId);

        /// <summary>
        /// Get vote statistics for a post
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <returns>Vote counts and score</returns>
        Task<(int upvotes, int downvotes, int score)> GetVoteStatsAsync(Guid postId);

        /// <summary>
        /// Check if user has voted on a post
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="postId">Post ID</param>
        /// <returns>True if user has voted</returns>
        Task<bool> HasUserVotedAsync(Guid userId, Guid postId);
    }
}