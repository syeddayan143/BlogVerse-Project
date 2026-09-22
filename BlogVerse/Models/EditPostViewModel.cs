using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogVerse.Models
{
    public class EditPostViewModel
    {
        [Required(ErrorMessage = "Please enter a title.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Please enter post content.")]
        public string? Content { get; set; }

        // Existing featured image
        public string? ImagePath { get; set; }

        // New featured image
        public IFormFile? Image { get; set; }

        // Existing uploaded file/document
        public string? AttachmentPath { get; set; }

        public string? AttachmentName { get; set; }

        // New file/document
        public IFormFile? Attachment { get; set; }
    }
}