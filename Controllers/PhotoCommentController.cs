using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using proiect.Models;
using System.Security.Claims;

namespace proiect.Controllers
{
    [Authorize]
    public class PhotoCommentController : Controller
    {
        private readonly ApplicationContext _context;

        public PhotoCommentController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int PhotoId, string Text, string? returnUrl)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new PhotoComment
            {
                PhotoId = PhotoId,
                Text = Text,
                CreatedAt = DateTime.Now,
                AuthorId = userId,
                IsApproved = false
            };

            _context.PhotoComments.Add(comment);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            var albumId = await _context.Photos
                .Where(p => p.Id == PhotoId)
                .Select(p => p.AlbumId)
                .FirstOrDefaultAsync();

            TempData["CommentConfirmation"] = "Your comment was submitted and is pending approval.";
            return Redirect(returnUrl ?? "/Home/Feed");

        }


        [Authorize]
        public async Task<IActionResult> Pending()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pendingComments = await _context.PhotoComments
                .Include(c => c.Author)
                .Include(c => c.Photo)
                    .ThenInclude(p => p.Album)
                .Where(c => !c.IsApproved && c.Photo.Album.UserId == userId)
                .ToListAsync();

            return View(pendingComments);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var comment = await _context.PhotoComments
                .Include(c => c.Photo)
                    .ThenInclude(p => p.Album)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.Photo.Album.UserId != userId)
                return Forbid();

            comment.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Pending");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var comment = await _context.PhotoComments
                .Include(c => c.Photo)
                    .ThenInclude(p => p.Album)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.Photo.Album.UserId != userId)
                return Forbid();

            _context.PhotoComments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Pending");
        }
    }
}