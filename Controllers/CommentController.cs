using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using proiect.Models;
using System.Security.Claims;

namespace proiect.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationContext _context;

        public CommentController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Comment comment, string returnUrl = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == comment.PostId);

            if (post == null)
                return NotFound();

            var isFriend = await _context.FriendRequests.AnyAsync(fr =>
                fr.IsAccepted &&
                ((fr.SenderId == userId && fr.ReceiverId == post.UserId) ||
                 (fr.ReceiverId == userId && fr.SenderId == post.UserId)));

            var canComment = post.User.IsProfilePublic || post.UserId == userId || isFriend;

            if (!canComment)
                return Forbid();

            comment.AuthorId = userId;
            comment.CreatedAt = DateTime.Now;
            comment.IsApproved = false;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["CommentConfirmation"] = "Your comment has been submitted for approval.";

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Post");
        }


        [Authorize]
        public async Task<IActionResult> Pending()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var pendingComments = await _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Post)
                .ThenInclude(p => p.User)
                .Where(c => !c.IsApproved && c.Post.UserId == userId)
                .ToListAsync();

            return View(pendingComments);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.Post.UserId != currentUserId)
                return Forbid();

            comment.IsApproved = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("Pending");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Post)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (comment.Post.UserId != currentUserId)
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Pending");
        }


    }
}