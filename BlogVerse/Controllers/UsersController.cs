using BlogVerse.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogVerse.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }


        // =====================================================
        // USER MANAGEMENT PAGE
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Index(
            string? search,
            string? status)
        {
            var query = _userManager.Users.AsNoTracking();


            // -------------------------------------------------
            // SEARCH
            // -------------------------------------------------

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    (u.Email != null &&
                     u.Email.Contains(search)));
            }


            // -------------------------------------------------
            // STATUS FILTER
            // -------------------------------------------------

            if (status == "active")
            {
                query = query.Where(u => u.IsActive);
            }
            else if (status == "inactive")
            {
                query = query.Where(u => !u.IsActive);
            }


            // -------------------------------------------------
            // GET USERS
            // -------------------------------------------------

            var users = await query
                .OrderByDescending(u => u.IsActive)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Email)
                .ToListAsync();


            // -------------------------------------------------
            // COUNTS
            // -------------------------------------------------

            ViewBag.Search = search;
            ViewBag.Status = status;

            ViewBag.TotalUsers =
                await _userManager.Users.CountAsync();

            ViewBag.ActiveUsers =
                await _userManager.Users
                    .CountAsync(u => u.IsActive);

            ViewBag.DeactivatedUsers =
                await _userManager.Users
                    .CountAsync(u => !u.IsActive);


            return View(users);
        }


        // =====================================================
        // USER DETAILS
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }


            // -------------------------------------------------
            // FIND USER
            // -------------------------------------------------

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }


            // -------------------------------------------------
            // GET USER POSTS
            // -------------------------------------------------

            var posts = await _context.Posts
                .AsNoTracking()
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();


            // -------------------------------------------------
            // CREATE VIEW MODEL
            // -------------------------------------------------

            var model = new UserDetailsViewModel
            {
                Id = user.Id,

                Name = user.Name,

                Email = user.Email ?? string.Empty,

                IsActive = user.IsActive,

                ProfileImagePath = user.ProfileImagePath,

                TotalPosts = posts.Count,

                PublishedPosts =
                    posts.Count(p => p.IsPublished),

                DraftPosts =
                    posts.Count(p => !p.IsPublished),

                TotalViews =
                    posts.Sum(p => p.Views),

                TotalLikes =
                    posts.Sum(p => p.Likes),

                Posts = posts.Select(p => new UserPostViewModel
                {
                    Id = p.Id,

                    Title = p.Title ?? "Untitled Post",

                    Category = p.Category ?? "Uncategorized",

                    Content = p.Content ?? string.Empty,

                    ImagePath = p.ImagePath,

                    // ==============================
                    // ATTACHMENT
                    // ==============================

                    AttachmentPath = p.AttachmentPath,

                    AttachmentName = p.AttachmentName,

                    // ==============================

                    IsPublished = p.IsPublished,

                    Views = p.Views,

                    Likes = p.Likes,

                    CreatedAt = p.CreatedAt,

                    PublishedAt = p.PublishedAt

                }).ToList()
            };


            return View(model);
        }


        // =====================================================
        // ACTIVATE / DEACTIVATE USER
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }


            var user =
                await _userManager.FindByIdAsync(id);


            if (user == null)
            {
                return NotFound();
            }


            // -------------------------------------------------
            // CURRENT ADMIN
            // -------------------------------------------------

            var currentUserId =
                _userManager.GetUserId(User);


            // Admin cannot deactivate himself
            if (user.Id == currentUserId)
            {
                TempData["Error"] =
                    "You cannot deactivate your own administrator account.";

                return RedirectToAction(nameof(Index));
            }


            // -------------------------------------------------
            // TOGGLE
            // -------------------------------------------------

            user.IsActive = !user.IsActive;


            var result =
                await _userManager.UpdateAsync(user);


            if (!result.Succeeded)
            {
                var errors =
                    string.Join(
                        ", ",
                        result.Errors.Select(
                            e => e.Description
                        )
                    );

                TempData["Error"] =
                    $"Unable to update user status: {errors}";

                return RedirectToAction(nameof(Index));
            }


            // -------------------------------------------------
            // LOGOUT DEACTIVATED USER
            // -------------------------------------------------

            if (!user.IsActive)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }


            TempData["Success"] =
                user.IsActive
                    ? $"{user.Name}'s account has been activated."
                    : $"{user.Name}'s account has been deactivated.";


            return RedirectToAction(nameof(Index));
        }


        // =====================================================
        // DELETE USER POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            // -------------------------------------------------
            // FIND POST
            // -------------------------------------------------

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id);


            if (post == null)
            {
                TempData["Error"] =
                    "Post not found.";

                return RedirectToAction(nameof(Index));
            }


            // -------------------------------------------------
            // SAVE USER ID BEFORE DELETE
            // -------------------------------------------------

            var userId = post.UserId;


            // -------------------------------------------------
            // DELETE POST
            // -------------------------------------------------

            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Post deleted successfully.";


            // -------------------------------------------------
            // RETURN TO USER DETAILS
            // -------------------------------------------------

            return RedirectToAction(
                nameof(Details),
                new { id = userId }
            );
        }
    }
}