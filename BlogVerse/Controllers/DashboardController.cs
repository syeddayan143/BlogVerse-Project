using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using BlogVerse.Models;
using BlogVerse.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using AppChannel = BlogVerse.Models.Channel;

namespace BlogVerse.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
        }

        // =========================================================
        // DASHBOARD
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var preference = await _context.DashboardPreferences
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                ViewBag.LayoutConfig = preference?.LayoutConfiguration ?? "{}";
                ViewBag.UserPreferences = preference; // Loaded for custom styles and widgets
            }
            else
            {
                ViewBag.LayoutConfig = "{}";
                ViewBag.UserPreferences = null;
            }

            var posts = await _context.Posts
                .AsNoTracking()
                .Include(p => p.User)
                .Where(p =>
                    p.IsPublished &&
                    p.User != null &&
                    p.User.IsActive)
                .OrderByDescending(
                    p => p.PublishedAt ?? p.CreatedAt)
                .ToListAsync();

            var model = new DashboardViewModel
            {
                FollowedChannels = GetMockChannels(),
                Posts = posts
            };

            return View(model);
        }

        // =========================================================
        // SAVE LAYOUT PREFERENCE [NEW]
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> SaveLayout([FromBody] string layoutConfig)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrEmpty(layoutConfig))
            {
                return BadRequest("Invalid layout configuration.");
            }

            var preference = await _context.DashboardPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preference == null)
            {
                preference = new UserDashboardPreference
                {
                    UserId = userId,
                    LayoutConfiguration = layoutConfig
                };
                _context.DashboardPreferences.Add(preference);
            }
            else
            {
                preference.LayoutConfiguration = layoutConfig;
                preference.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Layout saved successfully." });
        }

        // =========================================================
        // [NEW] SAVE FULL DASHBOARD CUSTOMIZATION SETTINGS
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCustomizationSettings(string themeColor, string accentColor, string fontFamily, string dashboardStyle, string customWidgetName)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var preference = await _context.DashboardPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (preference == null)
            {
                preference = new UserDashboardPreference
                {
                    UserId = userId,
                    ThemeColor = themeColor ?? "#0f172a",
                    AccentColor = accentColor ?? "#38bdf8",
                    FontFamily = fontFamily ?? "Segoe UI",
                    DashboardStyle = dashboardStyle ?? "glassmorphism",
                    LayoutConfiguration = "{}"
                };
                _context.DashboardPreferences.Add(preference);
            }
            else
            {
                preference.ThemeColor = themeColor ?? preference.ThemeColor;
                preference.AccentColor = accentColor ?? preference.AccentColor;
                preference.FontFamily = fontFamily ?? preference.FontFamily;
                preference.DashboardStyle = dashboardStyle ?? preference.DashboardStyle;

                if (!string.IsNullOrWhiteSpace(customWidgetName))
                {
                    var widgets = string.IsNullOrEmpty(preference.CustomWidgetsJson)
                        ? new List<string>()
                        : System.Text.Json.JsonSerializer.Deserialize<List<string>>(preference.CustomWidgetsJson) ?? new List<string>();

                    widgets.Add(customWidgetName.Trim());
                    preference.CustomWidgetsJson = System.Text.Json.JsonSerializer.Serialize(widgets);
                }

                preference.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Dashboard customization and theme updated successfully!";
            return RedirectToAction(nameof(Settings));
        }

        // =========================================================
        // ACCOUNT
        // =========================================================

        [HttpGet]
        public IActionResult Account()
        {
            ViewData["Title"] = "Account";
            return View();
        }

        // =========================================================
        // SETTINGS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = user.Id;
            var preference = await _context.DashboardPreferences
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            ViewBag.UserName = user.Name;
            ViewBag.UserEmail = user.Email;
            ViewBag.ProfileImagePath = user.ProfileImagePath;
            ViewBag.UserPreferences = preference; // Loaded into settings for customization

            ViewData["Title"] = "Settings";

            return View();
        }

        // =========================================================
        // UPLOAD PROFILE PHOTO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePhoto(
            IFormFile? ProfilePhoto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ProfilePhoto == null ||
                ProfilePhoto.Length == 0)
            {
                TempData["Error"] =
                    "Please select a profile photo.";

                return RedirectToAction(nameof(Settings));
            }

            if (ProfilePhoto.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] =
                    "Profile photo must be less than 5 MB.";

                return RedirectToAction(nameof(Settings));
            }

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".webp"
            };

            var extension =
                Path.GetExtension(ProfilePhoto.FileName)
                    .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] =
                    "Only JPG, JPEG, PNG, GIF and WEBP images are allowed.";

                return RedirectToAction(nameof(Settings));
            }

            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "profiles"
            );

            Directory.CreateDirectory(uploadFolder);

            // Delete old profile photo
            if (!string.IsNullOrWhiteSpace(
                user.ProfileImagePath))
            {
                var oldFileName =
                    Path.GetFileName(
                        user.ProfileImagePath);

                var oldFilePath =
                    Path.Combine(
                        uploadFolder,
                        oldFileName);

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProfilePhoto.CopyToAsync(stream);
            }

            user.ProfileImagePath = $"/uploads/profiles/{fileName}";
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile photo updated successfully.";
            return RedirectToAction(nameof(Settings));
        }

        // =========================================================
        // REMOVE PROFILE PHOTO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfilePhoto()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrWhiteSpace(
                user.ProfileImagePath))
            {
                var fileName =
                    Path.GetFileName(
                        user.ProfileImagePath);

                var filePath = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "profiles",
                    fileName
                );

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                user.ProfileImagePath = null;

                await _userManager.UpdateAsync(user);
            }

            TempData["Success"] =
                "Profile photo removed successfully.";

            return RedirectToAction(nameof(Settings));
        }

        // =========================================================
        // BREAKING NEWS
        // =========================================================

        [HttpGet]
        public IActionResult BreakingNews()
        {
            ViewData["Title"] = "Breaking News";
            return View();
        }

        // =========================================================
        // COMMUNITY
        // =========================================================

        [HttpGet]
        public IActionResult Community()
        {
            var availableCommunities =
                GetCommunities();

            ViewBag.AvailableCommunities =
                availableCommunities;

            return View();
        }

        // =========================================================
        // JOIN COMMUNITY
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult JoinCommunity(int communityId)
        {
            var community =
                GetCommunityById(communityId);

            if (community != null)
            {
                TempData["JoinMessage"] =
                    $"You have successfully joined the '{community.Name}' community!";
            }
            else
            {
                TempData["JoinMessage"] =
                    "Error: Community not found!";
            }

            return RedirectToAction(nameof(Community));
        }

        // =========================================================
        // MY POSTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> MyPosts()
        {
            var userId =
                _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var posts = await _context.Posts
                .Where(p =>
                    p.UserId == userId &&
                    p.IsPublished)
                .OrderByDescending(
                    p => p.PublishedAt ?? p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =========================================================
        // ANALYTICS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Analytics()
        {
            var userId =
                _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var posts = await _context.Posts
                .Where(p =>
                    p.UserId == userId &&
                    p.IsPublished)
                .OrderByDescending(
                    p => p.CreatedAt)
                .ToListAsync();

            return View(posts);
        }

        // =========================================================
        // TOGGLE FOLLOW
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleFollow(int channelId)
        {
            return Json(new
            {
                success = true,
                followed = true
            });
        }

        // =========================================================
        // MOCK CHANNELS
        // =========================================================

        private List<AppChannel> GetMockChannels()
        {
            return new List<AppChannel>
            {
                new AppChannel
                {
                    Id = 1,
                    Name = "Tech Digest",
                    IsFollowed = true
                },

                new AppChannel
                {
                    Id = 2,
                    Name = "Creative Writers",
                    IsFollowed = false
                },

                new AppChannel
                {
                    Id = 3,
                    Name = "Design Diaries",
                    IsFollowed = false
                }
            };
        }

        // =========================================================
        // COMMUNITIES
        // =========================================================

        private List<CommunityModel> GetCommunities()
        {
            return new List<CommunityModel>
            {
                new CommunityModel
                {
                    Id = 1,
                    Name = "Photography Enthusiasts",
                    Description =
                        "Share your best photos and discuss techniques.",
                    ImageUrl =
                        "~/images/Photography.png",
                    MemberCount = 150
                },

                new CommunityModel
                {
                    Id = 2,
                    Name = "Food Lovers United",
                    Description =
                        "A place to share recipes, restaurant reviews, and all things food!",
                    ImageUrl =
                        "~/images/foodie.png",
                    MemberCount = 280
                },

                new CommunityModel
                {
                    Id = 3,
                    Name = "Travel Explorers",
                    Description =
                        "Discuss your travel adventures, share tips, and plan your next trip.",
                    ImageUrl =
                        "~/images/travel.png",
                    MemberCount = 95
                },

                new CommunityModel
                {
                    Id = 4,
                    Name = "Book Worms Corner",
                    Description =
                        "Talk about your favorite books, authors, and literary discussions.",
                    ImageUrl =
                        "~/images/bookwormm.png",
                    MemberCount = 210
                }
            };
        }

        private CommunityModel? GetCommunityById(int id)
        {
            return GetCommunities()
                .FirstOrDefault(c => c.Id == id);
        }
    }
}