using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.Models;
using proiect.ContextModels;
using proiect.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace proiect.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [AllowAnonymous]
        public async Task<IActionResult> Feed()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            var currentUserId = _userManager.GetUserId(User);

            var friendIds = new List<string>();
            if (isAuthenticated)
            {
                friendIds = await _context.FriendRequests
                    .Where(fr => fr.IsAccepted &&
                        (fr.SenderId == currentUserId || fr.ReceiverId == currentUserId))
                    .Select(fr => fr.SenderId == currentUserId ? fr.ReceiverId : fr.SenderId)
                    .ToListAsync();
            }

            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.Author)
                .Where(p =>
                    p.User.IsProfilePublic ||
                    (isAuthenticated && (p.UserId == currentUserId || friendIds.Contains(p.UserId)))
                )
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var photos = await _context.Photos
                .Include(ph => ph.Album)
                    .ThenInclude(a => a.User)
                .Include(ph => ph.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.Author)
                .Where(ph =>
                    ph.Album.User.IsProfilePublic ||
                    (isAuthenticated && (ph.Album.UserId == currentUserId || friendIds.Contains(ph.Album.UserId)))
                )
                .OrderByDescending(ph => ph.Id)
                .ToListAsync();

            var albums = isAuthenticated
                ? await _context.Albums.Where(a => a.UserId == currentUserId).ToListAsync()
                : new List<Album>();

            var model = new FeedViewModel
            {
                Posts = posts,
                Photos = photos,
                Albums = albums
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPost(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return RedirectToAction("Feed");

            var userId = _userManager.GetUserId(User);

            var post = new Post
            {
                Text = text,
                CreatedAt = DateTime.Now,
                UserId = userId
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Feed");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhoto(IFormFile photoFile, string description, int? selectedAlbumId, string newAlbumTitle)
        {
            if (photoFile == null || photoFile.Length == 0)
                return RedirectToAction("Feed");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            Album album = null;

            if (!string.IsNullOrEmpty(newAlbumTitle))
            {
                album = new Album
                {
                    Title = newAlbumTitle,
                    UserId = user.Id
                };
                _context.Albums.Add(album);
                await _context.SaveChangesAsync();
            }
            else if (selectedAlbumId.HasValue)
            {
                album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == selectedAlbumId && a.UserId == user.Id);
                if (album == null) return Forbid();
            }
            else
            {
                album = await _context.Albums
                    .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Title == "Default");

                if (album == null)
                {
                    album = new Album
                    {
                        Title = "Default",
                        UserId = user.Id
                    };
                    _context.Albums.Add(album);
                    await _context.SaveChangesAsync();
                }
            }

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photoFile.CopyToAsync(stream);
            }

            var photo = new Photo
            {
                AlbumId = album.Id,
                ImageUrl = "/uploads/" + fileName
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Photo uploaded successfully.";
            return RedirectToAction("Feed");
        }
    }
}