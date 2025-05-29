namespace SkyPointSocial.Core.Entities
{
    /// <summary>
    /// Represents a follow relationship between two users in the social media platform.
    /// </summary>
    public class Follow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Follow"/> class.
        /// </summary>
        public Follow()
        {
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Follow"/> class with specified follower and following user.
        /// </summary>
        /// <param name="followerId">The identifier of the user who is following.</param>
        /// <param name="followingId">The identifier of the user being followed.</param>
        public Follow(Guid followerId, Guid followingId) : this()
        {
            Id = Guid.NewGuid();
            FollowerId = followerId;
            FollowingId = followingId;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the follow relationship.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who is following.
        /// </summary>
        public Guid FollowerId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user being followed.
        /// </summary>
        public Guid FollowingId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the follow relationship was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user who is following.
        /// </summary>
        public User Follower { get; set; }

        /// <summary>
        /// Gets or sets the user being followed.
        /// </summary>
        public User Following { get; set; }
    }
}