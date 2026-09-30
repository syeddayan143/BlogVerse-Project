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

        // =====================================================
        // BLOG POST
        // =====================================================

        public DbSet<Post> Posts { get; set; }


        // =====================================================
        // POST LIKE
        // =====================================================

        public DbSet<PostLike> PostLikes { get; set; }


        // =====================================================
        // COMMENTS
        // =====================================================

        public DbSet<Comment> Comments { get; set; }


        // =====================================================
        // DASHBOARD PREFERENCES
        // Existing functionality
        // =====================================================

        public DbSet<UserDashboardPreference> DashboardPreferences { get; set; }


        // =====================================================
        // POST VIEW HISTORY
        // Used by History page
        // =====================================================

        public DbSet<PostView> PostViews { get; set; }


        // =====================================================
        // SAVED POSTS
        // Used by Saved page
        // =====================================================

        public DbSet<SavedPost> SavedPosts { get; set; }


        // =====================================================
        // POST COLLECTIONS
        // Used by Collections page
        // =====================================================

        public DbSet<PostCollection> PostCollections { get; set; }


        // =====================================================
        // POSTS INSIDE COLLECTIONS
        // =====================================================

        public DbSet<CollectionPost> CollectionPosts { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            // =====================================================
            // BLOGVERSE IDENTITY TABLES
            // =====================================================

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
                .HasIndex(pl => new
                {
                    pl.PostId,
                    pl.UserId
                })
                .IsUnique();


            // =====================================================
            // USER DASHBOARD PREFERENCE
            // Existing functionality
            // =====================================================

            builder.Entity<UserDashboardPreference>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.UserId)
                    .IsUnique();

                entity.Property(e => e.LayoutConfiguration)
                    .IsRequired();
            });


            // =====================================================
            // POST VIEW HISTORY
            // =====================================================

            builder.Entity<PostView>()
                .HasOne(v => v.Post)
                .WithMany()
                .HasForeignKey(v => v.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<PostView>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Prevent the same user from creating
            // duplicate history records for the same post.

            builder.Entity<PostView>()
                .HasIndex(v => new
                {
                    v.PostId,
                    v.UserId
                })
                .IsUnique();


            // =====================================================
            // SAVED POSTS
            // =====================================================

            builder.Entity<SavedPost>()
                .HasOne(s => s.Post)
                .WithMany()
                .HasForeignKey(s => s.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<SavedPost>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // Prevent same user from saving
            // the same post more than once.

            builder.Entity<SavedPost>()
                .HasIndex(s => new
                {
                    s.PostId,
                    s.UserId
                })
                .IsUnique();


            // =====================================================
            // POST COLLECTION
            // =====================================================

            builder.Entity<PostCollection>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);


            // =====================================================
            // COLLECTION POST
            // =====================================================

            builder.Entity<CollectionPost>()
                .HasOne(cp => cp.Collection)
                .WithMany(c => c.Posts)
                .HasForeignKey(cp => cp.CollectionId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Entity<CollectionPost>()
                .HasOne(cp => cp.Post)
                .WithMany()
                .HasForeignKey(cp => cp.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            // Prevent adding the same post
            // to the same collection twice.

            builder.Entity<CollectionPost>()
                .HasIndex(cp => new
                {
                    cp.CollectionId,
                    cp.PostId
                })
                .IsUnique();
        }
    }
}