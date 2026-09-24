using System.Collections.Generic;
using BlogVerse.Models;

namespace BlogVerse.ViewModels
{
    public class DashboardViewModel
    {
        public List<Channel> FollowedChannels { get; set; }
            = new List<Channel>();

        public List<Post> Posts { get; set; }
            = new List<Post>();
    }
}