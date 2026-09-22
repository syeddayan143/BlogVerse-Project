using System;
using System.Collections.Generic;

namespace BlogVerse.Models
{
    public class UserDetailsViewModel
    {
        // ==============================
        // USER INFORMATION
        // ==============================

        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string? ProfileImagePath { get; set; }


        // ==============================
        // USER STATISTICS
        // ==============================

        public int TotalPosts { get; set; }

        public int PublishedPosts { get; set; }

        public int DraftPosts { get; set; }

        public int TotalViews { get; set; }

        public int TotalLikes { get; set; }


        // ==============================
        // USER POSTS
        // ==============================

        public List<UserPostViewModel> Posts { get; set; }
            = new List<UserPostViewModel>();
    }


    public class UserPostViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        // ==============================
        // POST IMAGE
        // ==============================

        public string? ImagePath { get; set; }


        // ==============================
        // ATTACHMENT
        // ==============================

        public string? AttachmentPath { get; set; }

        public string? AttachmentName { get; set; }


        // ==============================
        // POST STATUS
        // ==============================

        public bool IsPublished { get; set; }

        public int Views { get; set; }

        public int Likes { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }
    }
}