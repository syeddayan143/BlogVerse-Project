using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogVerse.Models
{
    public class PostCollection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }

        public ICollection<CollectionPost> Posts { get; set; }
            = new List<CollectionPost>();
    }
}