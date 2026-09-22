using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogVerse.Models
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Please enter a title.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Please enter post content.")]
        public string? Content { get; set; }

        // Featured Image
        public IFormFile? Image { get; set; }

        // Any allowed document/file
        public IFormFile? Attachment { get; set; }
    }
}