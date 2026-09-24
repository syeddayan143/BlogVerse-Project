using System;
using System.ComponentModel.DataAnnotations;

namespace BlogVerse.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        // Post on which the comment was made
        public int PostId { get; set; }

        // User who wrote the comment
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Comment text
        [Required]
        [StringLength(2000)]
        public string CommentText { get; set; } = string.Empty;

        // For future replies
        // Null means this is a normal comment
        // If it contains an Id, it is a reply to another comment
        public int? ParentCommentId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Relationships
        public Post Post { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;

        // Parent comment for replies
        public Comment? ParentComment { get; set; }

        // Replies to this comment
        public ICollection<Comment> Replies { get; set; }
            = new List<Comment>();
    }
}