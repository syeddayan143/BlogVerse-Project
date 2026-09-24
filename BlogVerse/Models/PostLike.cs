using System.ComponentModel.DataAnnotations;

namespace BlogVerse.Models
{
    public class PostLike
    {
        [Key]
        public int Id { get; set; }

        // Which post was liked
        public int PostId { get; set; }

        // Which user liked the post
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Relationship with Post
        public Post Post { get; set; } = null!;

        // Relationship with User
        public ApplicationUser User { get; set; } = null!;
    }
}