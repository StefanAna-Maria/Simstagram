using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using proiect.Models;

namespace proiect.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationContext _context;

        public AdminController(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var comments = await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Post)
                .ThenInclude(p => p.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var photos = await _context.Photos
                .Include(p => p.Album)
                .ThenInclude(a => a.User)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();

            var photoComments = await _context.PhotoComments
                .Include(pc => pc.Author)
                .Include(pc => pc.Photo)
                .ThenInclude(p => p.Album)
                .ThenInclude(a => a.User)
                .OrderByDescending(pc => pc.CreatedAt)
                .ToListAsync();

            ViewBag.Comments = comments;
            ViewBag.Photos = photos;
            ViewBag.PhotoComments = photoComments;

            return View(posts);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post != null)
            {
                var authorId = post.UserId;

                _context.Posts.Remove(post);

                _context.AdminNotifications.Add(new AdminNotification
                {
                    RecipientId = authorId,
                    Message = "Your post was removed by an admin.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment != null)
            {
                var authorId = comment.AuthorId;

                _context.Comments.Remove(comment);

                _context.AdminNotifications.Add(new AdminNotification
                {
                    RecipientId = authorId,
                    Message = "Your comment was removed by an admin.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await _context.Photos
                .Include(p => p.Album)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo != null)
            {
                var authorId = photo.Album.UserId;

                _context.Photos.Remove(photo);

                _context.AdminNotifications.Add(new AdminNotification
                {
                    RecipientId = authorId,
                    Message = "Your photo was removed by an admin.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }


        [HttpPost]
        public async Task<IActionResult> DeletePhotoComment(int id)
        {
            var comment = await _context.PhotoComments
                .Include(c => c.Author)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment != null)
            {
                var authorId = comment.AuthorId;

                _context.PhotoComments.Remove(comment);

                _context.AdminNotifications.Add(new AdminNotification
                {
                    RecipientId = authorId,
                    Message = "Your photo comment was removed by an admin.",
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> SendWarning()
        {
            var users = await _context.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();

            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendWarning(string recipientId, string message)
        {
            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(recipientId))
                return RedirectToAction("SendWarning");

            _context.AdminNotifications.Add(new AdminNotification
            {
                RecipientId = recipientId,
                Message = message,
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Warning sent successfully.";
            return RedirectToAction("Index");
        }




    }
}