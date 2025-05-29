using System.Xml.Linq;

namespace SkyPointSocial.Core.Entities
{
    /// <summary>
    /// Represents a user in the social media platform.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        public User()
        {
            Posts = new List<Post>();
            Comments = new List<Comment>();
            Votes = new List<Vote>();
            Followers = new List<Follow>();
            Following = new List<Follow>();
            Sessions = new List<Session>();
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class with specified email and username.
        /// </summary>
        /// <param name="email">The email of the user.</param>
        /// <param name="username">The username of the user.</param>
        public User(string email, string username) : this()
        {
            Id = Guid.NewGuid();
            Email = email;
            Username = username;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the user.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the email address of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// User's first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User's last name
        /// </summary>
        public string LastName { get; set; }

         /// <summary>
        /// URL to user's profile picture
        /// </summary>
        public string ProfilePictureUrl { get; set; }

        /// <summary>
        /// Gets or sets the password hash for the user.
        /// Null if the user authenticates via OAuth.
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the OAuth provider name (e.g., "Google", "Facebook").
        /// Null if the user authenticates with a password.
        /// </summary>
        public string? OAuthProvider { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier from the OAuth provider.
        /// Null if the user authenticates with a password.
        /// </summary>
        public string? OAuthProviderId { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the user was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update timestamp
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the collection of posts created by this user.
        /// </summary>
        public ICollection<Post> Posts { get; set; }

        /// <summary>
        /// Gets or sets the collection of comments created by this user.
        /// </summary>
        public ICollection<Comment> Comments { get; set; }

        /// <summary>
        /// Gets or sets the collection of votes cast by this user.
        /// </summary>
        public ICollection<Vote> Votes { get; set; }

        /// <summary>
        /// Gets or sets the collection of users who follow this user.
        /// </summary>
        public ICollection<Follow> Followers { get; set; }

        /// <summary>
        /// Gets or sets the collection of users that this user follows.
        /// </summary>
        public ICollection<Follow> Following { get; set; }

        /// <summary>
        /// Gets or sets the collection of sessions for this user.
        /// </summary>
        public ICollection<Session> Sessions { get; set; }
    }
}