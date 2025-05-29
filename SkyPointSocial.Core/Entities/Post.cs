using System.Xml.Linq;

namespace SkyPointSocial.Core.Entities
{
    /// <summary>
    /// Represents a post created by a user in the social media platform.
    /// </summary>
    public class Post
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Post"/> class.
        /// </summary>
        public Post()
        {
            Comments = new List<Comment>();
            Votes = new List<Vote>();
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            Score = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Post"/> class with specified user and content.
        /// </summary>
        /// <param name="userId">The identifier of the user creating the post.</param>
        /// <param name="content">The text content of the post.</param>
        public Post(Guid userId, string content) : this()
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Content = content;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the post.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created this post.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the text content of the post.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the score (upvotes minus downvotes) of the post.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the post was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the post was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user who created this post.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the collection of comments on this post.
        /// </summary>
        public ICollection<Comment> Comments { get; set; }

        /// <summary>
        /// Gets or sets the collection of votes on this post.
        /// </summary>
        public ICollection<Vote> Votes { get; set; }
    }
}