using Microsoft.AspNetCore.Identity;

namespace BlogVerse.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Profile photo path
        public string? ProfileImagePath { get; set; }
    }
}