using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using proiect.Models;
using System.Security.Claims;

namespace proiect.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        public PostController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            var currentUserId = isAuthenticated ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;

            var postsQuery = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .OrderByDescending(p => p.CreatedAt)
                .AsQueryable();

            if (!isAuthenticated)
            {
                postsQuery = postsQuery.Where(p => p.User.IsProfilePublic);
            }

            var allPosts = await postsQuery.ToListAsync();

            var acceptedFriendIds = new List<string>();
            if (isAuthenticated)
            {
                acceptedFriendIds = await _context.FriendRequests
                    .Where(fr =>
                        fr.IsAccepted &&
                        (fr.SenderId == currentUserId || fr.ReceiverId == currentUserId))
                    .Select(fr => fr.SenderId == currentUserId ? fr.ReceiverId : fr.SenderId)
                    .ToListAsync();
            }

            var visiblePosts = allPosts.Where(post =>
                isAuthenticated &&
                (post.UserId == currentUserId ||
                 acceptedFriendIds.Contains(post.UserId)) ||
                post.User.IsProfilePublic
            ).ToList();

            var allowedToComment = new Dictionary<int, bool>();
            foreach (var post in visiblePosts)
            {
                bool isOwner = post.UserId == currentUserId;
                bool isFriend = acceptedFriendIds.Contains(post.UserId);
                bool canComment = isAuthenticated && (post.User.IsProfilePublic || isFriend || isOwner);

                allowedToComment[post.Id] = canComment;
            }

            ViewBag.AllowedToComment = allowedToComment;

            return View(visiblePosts);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            if (ModelState.IsValid)
            {
                post.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                post.CreatedAt = DateTime.Now;
                _context.Posts.Add(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int postId, string content, string returnUrl)
        {
            var userId = _userManager.GetUserId(User);

            var comment = new Comment
            {
                PostId = postId,
                AuthorId = userId,
                Text = content,
                CreatedAt = DateTime.Now,
                IsApproved = false
            };


            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Redirect(returnUrl ?? "/Posts");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromProfile(string text)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["Error"] = "Post text cannot be empty.";
                return RedirectToAction("ViewProfile", "Profile", new { id = userId });
            }

            var post = new Post
            {
                Text = text,
                CreatedAt = DateTime.Now,
                UserId = userId
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewProfile", "Profile", new { id = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Comments)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (post.UserId != currentUserId)
            {
                return Forbid(); 
            }

            _context.Comments.RemoveRange(post.Comments);

            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();

            return RedirectToAction("ViewProfile", "Profile", new { id = currentUserId });

        }


    }
}