namespace SkyPointSocial.Core.Entities
{
    /// <summary>
    /// Represents a user comment on a post or a reply to another comment in the social media platform.
    /// </summary>
    public class Comment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Comment"/> class.
        /// </summary>
        public Comment()
        {
            Replies = new List<Comment>();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Comment"/> class with specified user, post, and content.
        /// </summary>
        /// <param name="userId">The identifier of the user creating the comment.</param>
        /// <param name="postId">The identifier of the post this comment belongs to.</param>
        /// <param name="content">The text content of the comment.</param>
        public Comment(Guid userId, Guid postId, string content) : this()
        {
            Id = Guid.NewGuid();
            UserId = userId;
            PostId = postId;
            Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Comment"/> class as a reply to another comment.
        /// </summary>
        /// <param name="userId">The identifier of the user creating the reply.</param>
        /// <param name="postId">The identifier of the post this reply belongs to.</param>
        /// <param name="parentCommentId">The identifier of the parent comment.</param>
        /// <param name="content">The text content of the reply.</param>
        public Comment(Guid userId, Guid postId, Guid parentCommentId, string content) : this(userId, postId, content)
        {
            ParentCommentId = parentCommentId;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the comment.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the post this comment belongs to.
        /// </summary>
        public Guid PostId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who created this comment.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the text content of the comment.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the parent comment if this is a reply.
        /// Null if this is a top-level comment.
        /// </summary>
        public Guid? ParentCommentId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the comment was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the post that this comment belongs to.
        /// </summary>
        public Post Post { get; set; }

        /// <summary>
        /// Gets or sets the user who created this comment.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Gets or sets the parent comment if this is a reply.
        /// </summary>
        public Comment ParentComment { get; set; }

        /// <summary>
        /// Gets or sets the collection of replies to this comment.
        /// </summary>
        public ICollection<Comment> Replies { get; set; }
    }
}