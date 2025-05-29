using System;

namespace SkyPointSocial.Core.ClientModels.Vote
{
    /// <summary>
    /// Model for voting on a post
    /// </summary>
    public class CreateVoteClientModel
    {
        /// <summary>
        /// ID of the post to vote on
        /// </summary>
        public Guid PostId { get; set; }

        /// <summary>
        /// Vote type: 1 for upvote, -1 for downvote
        /// Used to calculate post score
        /// </summary>
        public int VoteType { get; set; }
    }
}