using System;
using System.ComponentModel.DataAnnotations;

namespace BlogVerse.Models
{
    public class UserDashboardPreference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Customization Settings
        public string ThemeColor { get; set; } = "#0f172a";
        public string AccentColor { get; set; } = "#38bdf8";
        public string FontFamily { get; set; } = "Segoe UI";
        public string DashboardStyle { get; set; } = "glassmorphism";

        // JSON storage for Card Order & Widget Visibility
        public string LayoutConfiguration { get; set; } = "{}";

        // JSON storage for Custom Added Widgets
        public string CustomWidgetsJson { get; set; } = "[]";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}