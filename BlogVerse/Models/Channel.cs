using System.ComponentModel.DataAnnotations;

namespace BlogVerse.Models
{
    public class Channel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Channel name cannot exceed 100 characters.")]
        public string Name { get; set; }

        public bool IsFollowed { get; set; }
    }
}
