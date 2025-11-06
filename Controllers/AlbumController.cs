using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using proiect.Models;
using System.Security.Claims;

namespace proiect.Controllers
{
    [Authorize]
    public class AlbumController : Controller
    {
        private readonly ApplicationContext _context;

        public AlbumController(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> MyAlbums()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var albums = await _context.Albums
                .Where(a => a.UserId == userId)
                .Include(a => a.Photos)
                .ToListAsync();

            return View(albums);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Album album)
        {
            if (ModelState.IsValid)
            {
                album.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                _context.Albums.Add(album);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(MyAlbums));
            }
            return View(album);
        }

        public async Task<IActionResult> ViewAlbum(int id)
        {
            var album = await _context.Albums
                .Include(a => a.Photos)
                    .ThenInclude(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (album == null)
                return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isOwner = album.UserId == currentUserId;

            bool isFriend = await _context.FriendRequests.AnyAsync(fr =>
                fr.IsAccepted &&
                ((fr.SenderId == currentUserId && fr.ReceiverId == album.UserId) ||
                 (fr.ReceiverId == currentUserId && fr.SenderId == album.UserId)));

            bool canView = album.User.IsProfilePublic || isOwner || isFriend;

            if (!canView)
                return Forbid();

            ViewBag.CanComment = canView;
            ViewBag.CurrentUserId = currentUserId;

            return View(album);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var album = _context.Albums
                .Include(a => a.Photos)
                .FirstOrDefault(a => a.Id == id);

            if (album == null)
            {
                return NotFound();
            }

            if (album.Photos != null)
            {
                _context.Photos.RemoveRange(album.Photos);
            }

            _context.Albums.Remove(album);
            _context.SaveChanges();

            return RedirectToAction("ViewProfile", "Profile", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier) });
        }
    }
}