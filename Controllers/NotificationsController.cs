
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using proiect.ContextModels;
using proiect.Models;
using Microsoft.EntityFrameworkCore;
using proiect.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

[Authorize]
public class NotificationsController : Controller
{
    private readonly ApplicationContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(ApplicationContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentUserId = _userManager.GetUserId(User);

        var pendingFriendRequests = await _context.FriendRequests
            .Where(fr => fr.ReceiverId == currentUserId && !fr.IsAccepted)
            .ToListAsync();

        ViewBag.PendingFriendRequests = pendingFriendRequests;

        var pendingPostComments = await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .ThenInclude(p => p.User)
            .Where(c => !c.IsApproved && c.Post.UserId == currentUserId)
            .GroupBy(c => new { c.PostId, c.AuthorId })
            .Select(g => g.First())
            .ToListAsync();

        var pendingPhotoComments = await _context.PhotoComments
            .Include(c => c.Author)
            .Include(c => c.Photo)
                .ThenInclude(p => p.Album)
            .ThenInclude(a => a.User)
            .Where(c => !c.IsApproved && c.Photo.Album.UserId == currentUserId)
            .GroupBy(c => new { c.PhotoId, c.AuthorId })
            .Select(g => g.First())
            .ToListAsync();

        var adminWarnings = await _context.AdminNotifications
            .Where(n => n.RecipientId == currentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var model = new NotificationViewModel
        {
            PendingPostComments = pendingPostComments,
            PendingPhotoComments = pendingPhotoComments,
            AdminWarnings = adminWarnings
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteWarning(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var warning = await _context.AdminNotifications
            .FirstOrDefaultAsync(w => w.Id == id && w.RecipientId == userId);

        if (warning == null)
            return NotFound();

        _context.AdminNotifications.Remove(warning);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}