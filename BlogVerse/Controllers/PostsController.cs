using Microsoft.AspNetCore.Mvc;
using BlogVerse.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace BlogVerse.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Maximum attachment size = 25 MB
        private const long MaxAttachmentSize = 25 * 1024 * 1024;

        // Common safe file/document extensions
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
            string submitButton)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Validate attachment
            if (model.Attachment != null &&
                model.Attachment.Length > 0)
            {
                var attachmentValidation =
                    ValidateAttachment(model.Attachment);

                if (!string.IsNullOrEmpty(attachmentValidation))
                {
                    ModelState.AddModelError(
                        "Attachment",
                        attachmentValidation
                    );
                }
            }

            if (ModelState.IsValid)
            {
                var post = new Post
                {
                    Title = model.Title,

                    Content = model.Content ?? "",

                    Category = model.Category ?? "",

                    UserId = userId,

                    CreatedAt = DateTime.Now,

                    IsPublished = submitButton == "Publish",

                    PublishedAt =
                        submitButton == "Publish"
                            ? DateTime.Now
                            : null
                };

                // =================================================
                // IMAGE UPLOAD
                // =================================================

                if (model.Image != null &&
                    model.Image.Length > 0)
                {
                    var uploadsFolder =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            "posts"
                        );

                    Directory.CreateDirectory(
                        uploadsFolder
                    );

                    var extension =
                        Path.GetExtension(
                            model.Image.FileName
                        );

                    var uniqueFileName =
                        Guid.NewGuid().ToString()
                        + extension;

                    var filePath =
                        Path.Combine(
                            uploadsFolder,
                            uniqueFileName
                        );

                    using (var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create))
                    {
                        await model.Image.CopyToAsync(
                            stream
                        );
                    }

                    post.ImagePath =
                        "/images/posts/"
                        + uniqueFileName;
                }

                // =================================================
                // DOCUMENT / FILE UPLOAD
                // =================================================

                if (model.Attachment != null &&
                    model.Attachment.Length > 0)
                {
                    var attachmentsFolder =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads",
                            "attachments"
                        );

                    Directory.CreateDirectory(
                        attachmentsFolder
                    );

                    var extension =
                        Path.GetExtension(
                            model.Attachment.FileName
                        ).ToLowerInvariant();

                    // Generate safe unique filename
                    var uniqueFileName =
                        Guid.NewGuid().ToString()
                        + extension;

                    var filePath =
                        Path.Combine(
                            attachmentsFolder,
                            uniqueFileName
                        );

                    using (var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(
                            stream
                        );
                    }

                    // Save file path
                    post.AttachmentPath =
                        "/uploads/attachments/"
                        + uniqueFileName;

                    // Save original file name
                    post.AttachmentName =
                        Path.GetFileName(
                            model.Attachment.FileName
                        );
                }

                // =================================================
                // SAVE POST
                // =================================================

                _context.Posts.Add(post);

                await _context.SaveChangesAsync();

                return RedirectToAction("MyPosts");
            }

            return View(model);
        }

        // =========================================================
        // MY POSTS
        // =========================================================

        public IActionResult MyPosts()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            var myPosts =
                _context.Posts
                    .Where(p =>
                        p.UserId == userId &&
                        p.IsPublished)
                    .OrderByDescending(
                        p => p.CreatedAt
                    )
                    .ToList();

            return View(myPosts);
        }

        // =========================================================
        // SAVED DRAFTS
        // =========================================================

        public IActionResult SavedDrafts()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            var drafts =
                _context.Posts
                    .Where(p =>
                        p.UserId == userId &&
                        !p.IsPublished)
                    .OrderByDescending(
                        p => p.CreatedAt
                    )
                    .ToList();

            return View(drafts);
        }

        // =========================================================
        // PUBLISH DRAFT
        // =========================================================

        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

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

            post.PublishedAt =
                DateTime.Now;

            _context.Update(post);

            await _context.SaveChangesAsync();

            return RedirectToAction("SavedDrafts");
        }

        // =========================================================
        // ANALYTICS
        // =========================================================

        public IActionResult Analytics()
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            var posts =
                _context.Posts
                    .Where(p =>
                        p.UserId == userId &&
                        p.IsPublished)
                    .ToList();

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
                    ClaimTypes.NameIdentifier
                );

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            var model =
                new EditPostViewModel
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
                    ClaimTypes.NameIdentifier
                );

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            // Validate new attachment
            if (model.Attachment != null &&
                model.Attachment.Length > 0)
            {
                var attachmentValidation =
                    ValidateAttachment(
                        model.Attachment
                    );

                if (!string.IsNullOrEmpty(
                    attachmentValidation))
                {
                    ModelState.AddModelError(
                        "Attachment",
                        attachmentValidation
                    );
                }
            }

            if (ModelState.IsValid)
            {
                post.Title =
                    model.Title;

                post.Category =
                    model.Category;

                post.Content =
                    model.Content;

                // =================================================
                // IMAGE UPDATE
                // =================================================

                if (model.Image != null &&
                    model.Image.Length > 0)
                {
                    var uploadsFolder =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "images",
                            "posts"
                        );

                    Directory.CreateDirectory(
                        uploadsFolder
                    );

                    var extension =
                        Path.GetExtension(
                            model.Image.FileName
                        );

                    var uniqueFileName =
                        Guid.NewGuid().ToString()
                        + extension;

                    var filePath =
                        Path.Combine(
                            uploadsFolder,
                            uniqueFileName
                        );

                    using (var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create))
                    {
                        await model.Image.CopyToAsync(
                            stream
                        );
                    }

                    post.ImagePath =
                        "/images/posts/"
                        + uniqueFileName;
                }

                // =================================================
                // ATTACHMENT UPDATE
                // =================================================

                if (model.Attachment != null &&
                    model.Attachment.Length > 0)
                {
                    var attachmentsFolder =
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "uploads",
                            "attachments"
                        );

                    Directory.CreateDirectory(
                        attachmentsFolder
                    );

                    var extension =
                        Path.GetExtension(
                            model.Attachment.FileName
                        ).ToLowerInvariant();

                    var uniqueFileName =
                        Guid.NewGuid().ToString()
                        + extension;

                    var filePath =
                        Path.Combine(
                            attachmentsFolder,
                            uniqueFileName
                        );

                    using (var stream =
                        new FileStream(
                            filePath,
                            FileMode.Create))
                    {
                        await model.Attachment.CopyToAsync(
                            stream
                        );
                    }

                    // Delete old attachment
                    DeletePhysicalFile(
                        post.AttachmentPath
                    );

                    // Save new attachment
                    post.AttachmentPath =
                        "/uploads/attachments/"
                        + uniqueFileName;

                    post.AttachmentName =
                        Path.GetFileName(
                            model.Attachment.FileName
                        );
                }

                _context.Update(post);

                await _context.SaveChangesAsync();

                return RedirectToAction("MyPosts");
            }

            // Preserve existing values if validation fails
            model.ImagePath =
                post.ImagePath;

            model.AttachmentPath =
                post.AttachmentPath;

            model.AttachmentName =
                post.AttachmentName;

            return View(model);
        }

        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            var post =
                await _context.Posts
                    .FirstOrDefaultAsync(p =>
                        p.Id == id &&
                        p.UserId == userId);

            if (post == null)
            {
                return NotFound();
            }

            // Delete image
            DeletePhysicalFile(
                post.ImagePath
            );

            // Delete attachment
            DeletePhysicalFile(
                post.AttachmentPath
            );

            // Delete database record
            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();

            return RedirectToAction("MyPosts");
        }

        // =========================================================
        // VALIDATE ATTACHMENT
        // =========================================================

        private string? ValidateAttachment(
            IFormFile attachment)
        {
            // Check file size
            if (attachment.Length > MaxAttachmentSize)
            {
                return "File size cannot exceed 25 MB.";
            }

            // Get extension
            var extension =
                Path.GetExtension(
                    attachment.FileName
                ).ToLowerInvariant();

            // Check extension
            if (!AllowedAttachmentExtensions
                .Contains(extension))
            {
                return "This file type is not allowed.";
            }

            return null;
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
                        Path.DirectorySeparatorChar
                    )
                    .Replace(
                        '\\',
                        Path.DirectorySeparatorChar
                    );

            var fullPath =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    cleanPath
                );

            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch
                {
                    // Ignore physical file deletion errors
                }
            }
        }
    }
}