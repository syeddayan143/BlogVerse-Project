using BlogVerse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogVerse.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        // =====================================================
        // ADMIN DASHBOARD
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalUsers =
                await _userManager.Users.CountAsync();


            var activeUsers =
                await _userManager.Users
                    .CountAsync(u => u.IsActive);


            var deactivatedUsers =
                await _userManager.Users
                    .CountAsync(u => !u.IsActive);


            var totalPosts =
                await _context.Posts.CountAsync();


            var model = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                DeactivatedUsers = deactivatedUsers,
                TotalPosts = totalPosts
            };


            return View(model);
        }
    }
}