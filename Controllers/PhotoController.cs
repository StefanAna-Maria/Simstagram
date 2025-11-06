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
    public class PhotoController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PhotoController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Add(int albumId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == albumId && a.UserId == userId);
            if (album == null)
                return Forbid();

            ViewBag.AlbumId = albumId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int albumId, IFormFile imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var album = await _context.Albums.FirstOrDefaultAsync(a => a.Id == albumId && a.UserId == userId);
            if (album == null)
                return Forbid();

            if (imageFile == null || imageFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select an image.");
                ViewBag.AlbumId = albumId;
                return View();
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            var photo = new Photo
            {
                AlbumId = albumId,
                UploadedAt = DateTime.Now,
                ImageUrl = "/uploads/" + fileName
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return RedirectToAction("ViewAlbum", "Album", new { id = albumId });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var photo = _context.Photos
                .Include(p => p.Album)
                .FirstOrDefault(p => p.Id == id);

            if (photo == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (photo.Album.UserId != currentUserId)
                return Forbid();

            _context.Photos.Remove(photo);
            _context.SaveChanges();

            return RedirectToAction("ViewAlbum", "Album", new { id = photo.AlbumId });
        }



    }
}