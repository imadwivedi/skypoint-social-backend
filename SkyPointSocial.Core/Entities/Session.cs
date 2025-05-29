namespace SkyPointSocial.Core.Entities
{
    /// <summary>
    /// Represents a user session in the social media platform.
    /// </summary>
    public class Session
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class.
        /// </summary>
        public Session()
        {
            LoginTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Session"/> class with a specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user for this session.</param>
        public Session(Guid userId) : this()
        {
            Id = Guid.NewGuid();
            UserId = userId;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the session.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user associated with this session.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user logged in.
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user logged out.
        /// Null if the session is still active.
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// Gets or sets the duration of the session.
        /// Null if the session is still active.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        // Navigation properties

        /// <summary>
        /// Gets or sets the user associated with this session.
        /// </summary>
        public User User { get; set; }
    }
}