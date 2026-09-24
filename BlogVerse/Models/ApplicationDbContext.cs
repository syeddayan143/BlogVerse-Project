using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogVerse.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // BlogVerse Identity tables
            builder.Entity<ApplicationUser>()
                .ToTable("BlogVerseUsers");

            builder.Entity<IdentityRole>()
                .ToTable("BlogVerseRoles");

            builder.Entity<IdentityUserRole<string>>()
                .ToTable("BlogVerseUserRoles");

            builder.Entity<IdentityUserClaim<string>>()
                .ToTable("BlogVerseUserClaims");

            builder.Entity<IdentityUserLogin<string>>()
                .ToTable("BlogVerseUserLogins");

            builder.Entity<IdentityRoleClaim<string>>()
                .ToTable("BlogVerseRoleClaims");

            builder.Entity<IdentityUserToken<string>>()
                .ToTable("BlogVerseUserTokens");

            // =====================================================
            // POST LIKE RELATIONSHIP
            // =====================================================

            builder.Entity<PostLike>()
                .HasOne(pl => pl.Post)
                .WithMany()
                .HasForeignKey(pl => pl.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PostLike>()
                .HasOne(pl => pl.User)
                .WithMany()
                .HasForeignKey(pl => pl.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // COMMENT RELATIONSHIP
            // =====================================================

            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany()
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // COMMENT REPLY RELATIONSHIP
            // =====================================================

            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // PREVENT SAME USER FROM LIKING SAME POST TWICE
            // =====================================================

            builder.Entity<PostLike>()
                .HasIndex(pl => new { pl.PostId, pl.UserId })
                .IsUnique();
        }
    }
}