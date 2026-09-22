using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BlogVerse.Models;
using System.Collections.Generic;

// Alias the Channel model to avoid conflict with System.Threading.Channels
using AppChannel = BlogVerse.Models.Channel;
using BlogVerse.ViewModels;

namespace BlogVerse.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                FollowedChannels = GetMockChannels(),
                Posts = GetMockPosts()
            };

            return View(model);
        }

        public IActionResult Account()
        {
            ViewData["Title"] = "Account";
            return View();
        }

        // =====================================================
        // SETTINGS - GET
        // =====================================================

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.UserName = user.Name;
            ViewBag.UserEmail = user.Email;
            ViewBag.ProfileImagePath = user.ProfileImagePath;

            ViewData["Title"] = "Settings";

            return View();
        }

        // =====================================================
        // PROFILE PHOTO - UPLOAD
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePhoto(IFormFile? ProfilePhoto)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (ProfilePhoto == null || ProfilePhoto.Length == 0)
            {
                TempData["Error"] = "Please select a profile photo.";
                return RedirectToAction(nameof(Settings));
            }

            // Maximum 5 MB
            if (ProfilePhoto.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Profile photo must be less than 5 MB.";
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

            var extension = Path.GetExtension(ProfilePhoto.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["Error"] =
                    "Only JPG, JPEG, PNG, GIF and WEBP images are allowed.";

                return RedirectToAction(nameof(Settings));
            }

            // Create folder if it doesn't exist
            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "profiles"
            );

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // Delete old profile image
            if (!string.IsNullOrWhiteSpace(user.ProfileImagePath))
            {
                var oldFileName =
                    Path.GetFileName(user.ProfileImagePath);

                var oldFilePath = Path.Combine(
                    uploadFolder,
                    oldFileName
                );

                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Generate unique filename
            var fileName =
                $"{Guid.NewGuid()}{extension}";

            var filePath = Path.Combine(
                uploadFolder,
                fileName
            );

            // Save new image
            using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await ProfilePhoto.CopyToAsync(stream);
            }

            // Save path in database
            user.ProfileImagePath =
                $"/uploads/profiles/{fileName}";

            await _userManager.UpdateAsync(user);

            TempData["Success"] =
                "Profile photo updated successfully.";

            return RedirectToAction(nameof(Settings));
        }

        // =====================================================
        // PROFILE PHOTO - REMOVE
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProfilePhoto()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrWhiteSpace(user.ProfileImagePath))
            {
                var fileName =
                    Path.GetFileName(user.ProfileImagePath);

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

        // =====================================================
        // OTHER DASHBOARD ACTIONS
        // =====================================================

        public IActionResult BreakingNews()
        {
            ViewData["Title"] = "Breaking News";
            return View();
        }

        public IActionResult Community()
        {
            var availableCommunities = new List<CommunityModel>()
            {
                new CommunityModel
                {
                    Id = 1,
                    Name = "Photography Enthusiasts",
                    Description = "Share your best photos and discuss techniques.",
                    ImageUrl = "~/images/Photography.png",
                    MemberCount = 150
                },
                new CommunityModel
                {
                    Id = 2,
                    Name = "Food Lovers United",
                    Description = "A place to share recipes, restaurant reviews, and all things food!",
                    ImageUrl = "~/images/foodie.png",
                    MemberCount = 280
                },
                new CommunityModel
                {
                    Id = 3,
                    Name = "Travel Explorers",
                    Description = "Discuss your travel adventures, share tips, and plan your next trip.",
                    ImageUrl = "~/images/travel.png",
                    MemberCount = 95
                },
                new CommunityModel
                {
                    Id = 4,
                    Name = "Book Worms Corner",
                    Description = "Talk about your favorite books, authors, and literary discussions.",
                    ImageUrl = "~/images/bookwormm.png",
                    MemberCount = 210
                }
            };

            ViewBag.AvailableCommunities = availableCommunities;

            return View();
        }

        [HttpPost]
        public IActionResult JoinCommunity(int communityId)
        {
            var community = GetCommunityById(communityId);

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

            return RedirectToAction("Community");
        }

        public IActionResult MyPosts()
        {
            var posts = GetMockPosts();

            return View(posts);
        }

        public IActionResult Analytics()
        {
            var posts = GetMockPosts();

            return View(posts);
        }

        [HttpPost]
        public IActionResult ToggleFollow(int channelId)
        {
            return Json(new
            {
                success = true,
                followed = true
            });
        }

        private List<AppChannel> GetMockChannels() =>
            new List<AppChannel>
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

        private List<Post> GetMockPosts() =>
            new List<Post>
            {
                new Post
                {
                    Id = 1,
                    Title = "AI Writing Tips",
                    Views = 120,
                    Likes = 25
                },

                new Post
                {
                    Id = 2,
                    Title = "React vs ASP.NET",
                    Views = 80,
                    Likes = 12
                }
            };

        private CommunityModel GetCommunityById(int id)
        {
            var availableCommunities = new List<CommunityModel>()
            {
                new CommunityModel
                {
                    Id = 1,
                    Name = "Photography Enthusiasts",
                    Description = "Share your best photos and discuss techniques.",
                    ImageUrl = "~/images/Photography.png",
                    MemberCount = 150
                },

                new CommunityModel
                {
                    Id = 2,
                    Name = "Food Lovers United",
                    Description = "A place to share recipes, restaurant reviews, and all things food!",
                    ImageUrl = "~/images/foodie.png",
                    MemberCount = 280
                },

                new CommunityModel
                {
                    Id = 3,
                    Name = "Travel Explorers",
                    Description = "Discuss your travel adventures, share tips, and plan your next trip.",
                    ImageUrl = "~/images/travel.png",
                    MemberCount = 95
                },

                new CommunityModel
                {
                    Id = 4,
                    Name = "Book Worms Corner",
                    Description = "Talk about your favorite books, authors, and literary discussions.",
                    ImageUrl = "~/images/bookwormm.png",
                    MemberCount = 210
                }
            };

            return availableCommunities
                .FirstOrDefault(c => c.Id == id);
        }
    }
}