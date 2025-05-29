namespace SkyPointSocial.Core.Entities
{
    /// <summary>
    /// Represents a vote cast by a user on a post in the social media platform.
    /// </summary>
    public class Vote
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vote"/> class.
        /// </summary>
        public Vote()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vote"/> class with specified user, post, and vote type.
        /// </summary>
        /// <param name="userId">The identifier of the user casting the vote.</param>
        /// <param name="postId">The identifier of the post being voted on.</param>
        /// <param name="type">The type of vote (upvote or downvote).</param>
        public Vote(Guid userId, Guid postId, VoteType type) : this()
        {
            Id = Guid.NewGuid();
            UserId = userId;
            PostId = postId;
            Type = type;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the vote.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who cast this vote.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the post that was voted on.
        /// </summary>
        public Guid PostId { get; set; }

        /// <summary>
        /// Gets or sets the type of the vote (upvote or downvote).
        /// </summary>
        public VoteType Type { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the vote was cast.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user who cast this vote.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the post that was voted on.
        /// </summary>
        public Post Post { get; set; }
    }

    /// <summary>
    /// Defines the types of votes that can be cast on a post.
    /// </summary>
    public enum VoteType
    {
        /// <summary>
        /// A positive vote that increases the post's score.
        /// </summary>
        Upvote = 1,

        /// <summary>
        /// A negative vote that decreases the post's score.
        /// </summary>
        Downvote = -1
    }
}