using Microsoft.EntityFrameworkCore;
using SkyPointSocial.Core.Entities;

namespace SkyPointSocial.Application.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasIndex(e => e.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");

                entity.HasIndex(e => e.Username)
                    .IsUnique() // Username should be unique!
                    .HasDatabaseName("IX_Users_Username");

                // Add required string lengths
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).HasMaxLength(255);
                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500).IsRequired(false);
                entity.Property(e => e.OAuthProvider).HasMaxLength(50).IsRequired(false);
                entity.Property(e => e.OAuthProviderId).HasMaxLength(255).IsRequired(false);

                // PostgreSQL timestamp configuration
                if (Database.IsNpgsql())
                {
                    entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                    entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
                }
            });

            // Post entity
            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Content).IsRequired().HasMaxLength(5000);
                entity.Property(e => e.Score).HasDefaultValue(0);

                entity.HasOne(p => p.User)
                    .WithMany(u => u.Posts)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Cascade); // Add cascade delete

                // Add indexes for feed queries
                entity.HasIndex(p => p.CreatedAt)
                    .HasDatabaseName("IX_Posts_CreatedAt");

                entity.HasIndex(p => p.Score)
                    .HasDatabaseName("IX_Posts_Score");

                entity.HasIndex(p => new { p.UserId, p.CreatedAt })
                    .HasDatabaseName("IX_Posts_UserId_CreatedAt");

                if (Database.IsNpgsql())
                {
                    entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                    entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
                }
            });

            // Comment entity
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Content).IsRequired().HasMaxLength(2000);

                entity.HasOne(c => c.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.ParentComment)
                    .WithMany(c => c.Replies)
                    .HasForeignKey(c => c.ParentCommentId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete for replies

                // Add index for faster comment retrieval by post
                entity.HasIndex(c => new { c.PostId, c.CreatedAt })
                    .HasDatabaseName("IX_Comments_PostId_CreatedAt");

                if (Database.IsNpgsql())
                {
                    entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                }
            });

            // Vote entity
            modelBuilder.Entity<Vote>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Configure enum conversion
                entity.Property(e => e.Type)
                    .HasConversion<int>()
                    .IsRequired();

                entity.HasOne(v => v.User)
                    .WithMany(u => u.Votes)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.Post)
                    .WithMany(p => p.Votes)
                    .HasForeignKey(v => v.PostId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(v => new { v.UserId, v.PostId })
                    .IsUnique()
                    .HasDatabaseName("IX_Votes_UserId_PostId");

                // Add index for vote lookups
                entity.HasIndex(v => v.PostId)
                    .HasDatabaseName("IX_Votes_PostId");

                if (Database.IsNpgsql())
                {
                    entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                }
            });

            // Follow entity
            modelBuilder.Entity<Follow>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(f => f.Follower)
                    .WithMany(u => u.Following)
                    .HasForeignKey(f => f.FollowerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.Following)
                    .WithMany(u => u.Followers)
                    .HasForeignKey(f => f.FollowingId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(f => new { f.FollowerId, f.FollowingId })
                    .IsUnique()
                    .HasDatabaseName("IX_Follows_FollowerId_FollowingId");

                // Add indexes for follow queries
                entity.HasIndex(f => f.FollowerId)
                    .HasDatabaseName("IX_Follows_FollowerId");

                entity.HasIndex(f => f.FollowingId)
                    .HasDatabaseName("IX_Follows_FollowingId");

                if (Database.IsNpgsql())
                {
                    entity.Property(e => e.CreatedAt).HasColumnType("timestamp with time zone");
                }
            });

            // Session entity
            modelBuilder.Entity<Session>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(s => s.User)
                    .WithMany(u => u.Sessions)
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add index for active session lookups
                entity.HasIndex(s => new { s.UserId, s.LogoutTime })
                    .HasDatabaseName("IX_Sessions_UserId_LogoutTime");

                entity.HasIndex(s => s.LoginTime)
                    .HasDatabaseName("IX_Sessions_LoginTime");

                if (Database.IsNpgsql())
                {
                    entity.Property(e => e.LoginTime).HasColumnType("timestamp with time zone");
                    entity.Property(e => e.LogoutTime).HasColumnType("timestamp with time zone");
                }
            });
        }
    }
}