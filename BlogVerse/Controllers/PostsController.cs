using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using BlogVerse.Models;

using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlogVerse.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const long MaxAttachmentSize =
            25 * 1024 * 1024;

        private const long MaxImageSize =
            5 * 1024 * 1024;

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp"
        };

        private static readonly string[] AllowedAttachmentExtensions =
        {
            ".pdf",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx",
            ".csv",
            ".ppt",
            ".pptx",
            ".txt",
            ".rtf",
            ".json",
            ".xml",
            ".zip",
            ".rar",
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp",
            ".mp3",
            ".wav",
            ".mp4",
            ".mov",
            ".avi"
        };

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            CreatePostViewModel model,
            string? submitButton)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (model.Image != null &&
                model.Image.Length > 0)
            {
                var imageError =
                    ValidateImage(model.Image);

                if (!string.IsNullOrWhiteSpace(imageError))
                {
                    ModelState.AddModelError(
                        nameof(model.Image),
                        imageError);
                }
            }

            if (model.Attachment != null &&
                model.Attachment.Length > 0)
            {
                var attachmentError =
                    ValidateAttachment(model.Attachment);

                if (!string.IsNullOrWhiteSpace(attachmentError))
                {
                    ModelState.AddModelError(
                        nameof(model.Attachment),
                        attachmentError);
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isPublished =
                string.Equals(
                    submitButton,
                    "Publish",
                    StringComparison.OrdinalIgnoreCase);

            var post = new Post
            {
                Title = model.Title?.Trim(),
                Category = model.Category?.Trim(),
                Content = model.Content ?? string.Empty,

                UserId = userId,

                CreatedAt = DateTime.Now,

                IsPublished = isPublished,

                PublishedAt =
                    isPublished
                        ? DateTime.Now
                        : null
            };

            if (model.Image != null &&
                model.Image.Length > 0)
            {
                post.ImagePath =
                    await SaveImageAsync(model.Image);
            }

            if (model.Attachment != null &&
                model.Attachment.Length > 0)
            {
                var attachment =
                    await SaveAttachmentAsync(
                        model.Attachment);

                post.AttachmentPath =
                    attachment.Path;

                post.AttachmentName =
                    attachment.Name;
            }

            _context.Posts.Add(post);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(MyPosts));
        }

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p =>
                    p.Id == id &&
                    p.IsPublished &&
                    p.User != null &&
                    p.User.IsActive);

            if (post == null)
            {
                return NotFound();
            }

            // Increase views
            post.Views++;

            await _context.SaveChangesAsync();

            // =====================================================
            // LIKE INFORMATION
            // =====================================================

            var likeCount =
                await _context.PostLikes
                    .CountAsync(x =>
                        x.PostId == id);

            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            var isLiked =
                !string.IsNullOrEmpty(userId) &&
                await _context.PostLikes.AnyAsync(x =>
                    x.PostId == id &&
                    x.UserId == userId);

            ViewBag.LikeCount = likeCount;
            ViewBag.IsLiked = isLiked;

            // =====================================================
            // COMMENTS
            // =====================================================

            var comments =
                await _context.Comments
                    .Include(c => c.User)
                    .Where(c => c.PostId == id)
                    .OrderBy(c => c.CreatedAt)
                    .ToListAsync();

            ViewBag.Comments = comments;

            return View(post);
        }

        // =========================================================
        // LIKE / UNLIKE
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.IsPublished);

            if (post == null)
            {
                return NotFound();
            }

            var existingLike =
                await _context.PostLikes
                    .FirstOrDefaultAsync(x =>
                        x.PostId == id &&
                        x.UserId == userId);

            if (existingLike != null)
            {
                _context.PostLikes.Remove(
                    existingLike);
            }
            else
            {
                var newLike = new PostLike
                {
                    PostId = id,
                    UserId = userId
                };

                _context.PostLikes.Add(newLike);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = id });
        }

        // =========================================================
        // ADD COMMENT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(
            int postId,
            string commentText)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["CommentError"] =
                    "Please write a comment.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = postId });
            }

            commentText =
                commentText.Trim();

            if (commentText.Length > 2000)
            {
                TempData["CommentError"] =
                    "Comment cannot exceed 2000 characters.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = postId });
            }

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == postId &&
                        p.IsPublished);

            if (post == null)
            {
                return NotFound();
            }

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                CommentText = commentText,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = postId });
        }

        // =========================================================
        // REPLY TO COMMENT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReplyComment(
            int postId,
            int parentCommentId,
            string commentText)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(commentText))
            {
                TempData["CommentError"] =
                    "Please write a reply.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = postId });
            }

            commentText =
                commentText.Trim();

            if (commentText.Length > 2000)
            {
                TempData["CommentError"] =
                    "Reply cannot exceed 2000 characters.";

                return RedirectToAction(
                    nameof(Details),
                    new { id = postId });
            }

            var parentComment =
                await _context.Comments
                    .FirstOrDefaultAsync(c =>
                        c.Id == parentCommentId &&
                        c.PostId == postId);

            if (parentComment == null)
            {
                return NotFound();
            }

            var reply = new Comment
            {
                PostId = postId,
                UserId = userId,
                CommentText = commentText,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(reply);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = postId });
        }

        // =========================================================
        // DELETE COMMENT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(
            int id,
            int postId)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var comment =
                await _context.Comments
                    .FirstOrDefaultAsync(c =>
                        c.Id == id &&
                        c.PostId == postId &&
                        c.UserId == userId);

            if (comment == null)
            {
                return NotFound();
            }

            // Delete replies first
            var replies =
                await _context.Comments
                    .Where(c =>
                        c.ParentCommentId == id)
                    .ToListAsync();

            if (replies.Any())
            {
                _context.Comments.RemoveRange(
                    replies);
            }

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = postId });
        }

        // =========================================================
        // MY POSTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> MyPosts()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

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
        // SAVED DRAFTS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> SavedDrafts()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var drafts = await _context.Posts
                .Where(p =>
                    p.UserId == userId &&
                    !p.IsPublished)
                .OrderByDescending(
                    p => p.CreatedAt)
                .ToListAsync();

            return View(drafts);
        }

        // =========================================================
        // PUBLISH DRAFT
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId &&
                        !p.IsPublished);

            if (post == null)
            {
                return NotFound();
            }

            post.IsPublished = true;
            post.PublishedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(SavedDrafts));
        }

        // =========================================================
        // ANALYTICS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Analytics()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

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
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            var model = new EditPostViewModel
            {
                Title = post.Title,
                Category = post.Category,
                Content = post.Content,

                ImagePath = post.ImagePath,

                AttachmentPath =
                    post.AttachmentPath,

                AttachmentName =
                    post.AttachmentName
            };

            return View(model);
        }

        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            EditPostViewModel model)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            if (model.Image != null &&
                model.Image.Length > 0)
            {
                var imageError =
                    ValidateImage(model.Image);

                if (!string.IsNullOrWhiteSpace(imageError))
                {
                    ModelState.AddModelError(
                        nameof(model.Image),
                        imageError);
                }
            }

            if (model.Attachment != null &&
                model.Attachment.Length > 0)
            {
                var attachmentError =
                    ValidateAttachment(
                        model.Attachment);

                if (!string.IsNullOrWhiteSpace(
                    attachmentError))
                {
                    ModelState.AddModelError(
                        nameof(model.Attachment),
                        attachmentError);
                }
            }

            if (!ModelState.IsValid)
            {
                model.ImagePath =
                    post.ImagePath;

                model.AttachmentPath =
                    post.AttachmentPath;

                model.AttachmentName =
                    post.AttachmentName;

                return View(model);
            }

            post.Title =
                model.Title?.Trim();

            post.Category =
                model.Category?.Trim();

            post.Content =
                model.Content ?? string.Empty;

            if (model.Image != null &&
                model.Image.Length > 0)
            {
                var oldImage =
                    post.ImagePath;

                post.ImagePath =
                    await SaveImageAsync(
                        model.Image);

                DeletePhysicalFile(oldImage);
            }

            if (model.Attachment != null &&
                model.Attachment.Length > 0)
            {
                var oldAttachment =
                    post.AttachmentPath;

                var attachment =
                    await SaveAttachmentAsync(
                        model.Attachment);

                post.AttachmentPath =
                    attachment.Path;

                post.AttachmentName =
                    attachment.Name;

                DeletePhysicalFile(
                    oldAttachment);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(MyPosts));
        }

        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteConfirmed(int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            DeletePhysicalFile(
                post.ImagePath);

            DeletePhysicalFile(
                post.AttachmentPath);

            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(MyPosts));
        }

        // =========================================================
        // IMAGE VALIDATION
        // =========================================================

        private string? ValidateImage(
            IFormFile image)
        {
            if (image.Length > MaxImageSize)
            {
                return "Image size cannot exceed 5 MB.";
            }

            var extension =
                Path.GetExtension(
                    image.FileName)
                    .ToLowerInvariant();

            if (!AllowedImageExtensions
                .Contains(extension))
            {
                return
                    "Only JPG, JPEG, PNG, GIF and WEBP images are allowed.";
            }

            return null;
        }

        // =========================================================
        // ATTACHMENT VALIDATION
        // =========================================================

        private string? ValidateAttachment(
            IFormFile attachment)
        {
            if (attachment.Length >
                MaxAttachmentSize)
            {
                return "File size cannot exceed 25 MB.";
            }

            var extension =
                Path.GetExtension(
                    attachment.FileName)
                    .ToLowerInvariant();

            if (!AllowedAttachmentExtensions
                .Contains(extension))
            {
                return "This file type is not allowed.";
            }

            return null;
        }

        // =========================================================
        // SAVE IMAGE
        // =========================================================

        private async Task<string>
            SaveImageAsync(IFormFile image)
        {
            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "images",
                "posts");

            Directory.CreateDirectory(folder);

            var extension =
                Path.GetExtension(
                    image.FileName)
                    .ToLowerInvariant();

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var filePath =
                Path.Combine(
                    folder,
                    fileName);

            using (var stream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/images/posts/{fileName}";
        }

        // =========================================================
        // SAVE ATTACHMENT
        // =========================================================

        private async Task<AttachmentSaveResult>
            SaveAttachmentAsync(
                IFormFile attachment)
        {
            var folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads",
                "attachments");

            Directory.CreateDirectory(folder);

            var extension =
                Path.GetExtension(
                    attachment.FileName)
                    .ToLowerInvariant();

            var fileName =
                $"{Guid.NewGuid():N}{extension}";

            var filePath =
                Path.Combine(
                    folder,
                    fileName);

            using (var stream =
                   new FileStream(
                       filePath,
                       FileMode.Create))
            {
                await attachment.CopyToAsync(stream);
            }

            return new AttachmentSaveResult
            {
                Path =
                    $"/uploads/attachments/{fileName}",

                Name =
                    Path.GetFileName(
                        attachment.FileName)
            };
        }

        // =========================================================
        // DELETE PHYSICAL FILE
        // =========================================================

        private void DeletePhysicalFile(
            string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(
                relativePath))
            {
                return;
            }

            var cleanPath =
                relativePath
                    .TrimStart('/', '\\')
                    .Replace(
                        '/',
                        Path.DirectorySeparatorChar)
                    .Replace(
                        '\\',
                        Path.DirectorySeparatorChar);

            var fullPath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    cleanPath);

            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(
                        fullPath);
                }
                catch
                {
                    // Ignore deletion errors
                }
            }
        }

        // =========================================================
        // ATTACHMENT RESULT
        // =========================================================

        private class AttachmentSaveResult
        {
            public string Path { get; set; }
                = string.Empty;

            public string Name { get; set; }
                = string.Empty;
        }
    }
}