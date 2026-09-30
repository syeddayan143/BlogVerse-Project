using System.ComponentModel.DataAnnotations;

namespace BlogVerse.Models
{
    public class CollectionPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CollectionId { get; set; }

        [Required]
        public int PostId { get; set; }

        public PostCollection? Collection { get; set; }

        public Post? Post { get; set; }
    }
}