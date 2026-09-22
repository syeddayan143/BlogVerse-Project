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
        }
    }
}