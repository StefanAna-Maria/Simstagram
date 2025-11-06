using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using proiect.Models;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;


namespace proiect.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;


        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationContext context, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string search)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.DisplayName.Contains(search));
            }

            var result = await users.ToListAsync();
            return View(result);
        }


        public async Task<IActionResult> ViewProfile(string id)
        {
            var profileUser = await _context.Users
                .Include(u => u.Albums)
                    .ThenInclude(a => a.Photos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (profileUser == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);

            bool isFriend = await _context.FriendRequests
                .AnyAsync(fr => fr.IsAccepted &&
                    ((fr.SenderId == currentUserId && fr.ReceiverId == id) ||
                     (fr.SenderId == id && fr.ReceiverId == currentUserId)));

            ViewBag.IsFriend = isFriend;

            var pendingRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr =>
                    !fr.IsAccepted &&
                    ((fr.SenderId == currentUserId && fr.ReceiverId == id) ||
                    (fr.SenderId == id && fr.ReceiverId == currentUserId)));

            ViewBag.HasPendingFriendRequest = pendingRequest != null;
            ViewBag.SentByCurrentUser = pendingRequest?.SenderId == currentUserId;



            bool isOwner = profileUser.Id == currentUserId;
            bool isAdmin = User.IsInRole("Admin");

            bool canViewPosts = profileUser.IsProfilePublic || isOwner || isFriend || isAdmin;

            List<Post> posts = new List<Post>();
            if (canViewPosts)
            {
                posts = await _context.Posts
                    .Where(p => p.UserId == profileUser.Id)
                    .Include(p => p.Comments.Where(c => c.IsApproved))
                        .ThenInclude(c => c.Author)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

            }

            ViewBag.ProfilePosts = posts;
            ViewBag.Restricted = !canViewPosts;
            ViewBag.CurrentUserId = currentUserId;

            return View("ProfileView", profileUser);
        }


        [Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditProfile(ApplicationUser model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound();

            if (model.ProfilePictureFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images/profile_pics");
                Directory.CreateDirectory(uploadsFolder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePictureFile.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfilePictureFile.CopyToAsync(stream);
                }

                user.ProfilePicturePath = "/images/profile_pics/" + fileName;
            }

            user.DisplayName = model.DisplayName;
            user.IsProfilePublic = model.IsProfilePublic;

            await _userManager.UpdateAsync(user);
            return RedirectToAction("MyProfile");
        }



        [Authorize]
        public async Task<IActionResult> MyProfile()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            // Refolosește ViewProfile cu id-ul propriu
            return RedirectToAction("ViewProfile", new { id = currentUser.Id });
        }
    }
}