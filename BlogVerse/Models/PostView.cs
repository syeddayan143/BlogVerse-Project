using System;
using System.ComponentModel.DataAnnotations;

namespace BlogVerse.Models
{
    public class PostView
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;

        public Post? Post { get; set; }

        public ApplicationUser? User { get; set; }
    }
}