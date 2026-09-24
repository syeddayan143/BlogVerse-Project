using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlogVerse.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string? Filter { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Category { get; set; }

        [Required]
        public string? Content { get; set; }

        public string? ImagePath { get; set; }

        public string? AttachmentPath { get; set; }

        public string? AttachmentName { get; set; }

        [Required]
        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

        public bool IsPublished { get; set; } = false;

        public int Views { get; set; } = 0;

        public int Likes { get; set; } = 0;

        public DateTime CreatedAt { get; set; }
            = DateTime.Now;

        public DateTime? PublishedAt { get; set; }
    }
}