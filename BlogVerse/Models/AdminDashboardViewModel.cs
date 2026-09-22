namespace BlogVerse.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int DeactivatedUsers { get; set; }

        public int TotalPosts { get; set; }
    }
}