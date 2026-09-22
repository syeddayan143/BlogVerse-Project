using System.Collections.Generic; // For List<T>
using BlogVerse.Models;           // For Channel and Post model references

namespace BlogVerse.ViewModels
{
    public class DashboardViewModel
    {
        public List<Channel> FollowedChannels { get; set; }
        public List<Post> Posts { get; set; }
    }
}
