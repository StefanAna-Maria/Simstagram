using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using proiect.Models;

namespace proiect.Controllers
{
    [Authorize]
    public class FriendController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FriendController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> SendRequest(string receiverId)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (sender == null || receiverId == sender.Id)
                return BadRequest();

            var alreadyExists = await _context.FriendRequests.AnyAsync(fr =>
                (fr.SenderId == sender.Id && fr.ReceiverId == receiverId) ||
                (fr.SenderId == receiverId && fr.ReceiverId == sender.Id));

            if (!alreadyExists)
            {
                var request = new FriendRequest
                {
                    SenderId = sender.Id,
                    ReceiverId = receiverId,
                    IsAccepted = false
                };

                _context.FriendRequests.Add(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewProfile", "Profile", new { id = receiverId });
        }

        public async Task<IActionResult> ReceivedRequests()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var requests = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Where(fr => fr.ReceiverId == currentUser.Id && !fr.IsAccepted)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptRequest(int requestId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.ReceiverId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            request.IsAccepted = true;
            await _context.SaveChangesAsync();

            return RedirectToAction("ReceivedRequests");
        }

        public async Task<IActionResult> FriendsList()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var acceptedRequests = await _context.FriendRequests
                .Include(fr => fr.Sender)
                .Include(fr => fr.Receiver)
                .Where(fr =>
                    (fr.SenderId == currentUser.Id || fr.ReceiverId == currentUser.Id) &&
                    fr.IsAccepted)
                .ToListAsync();

            var friends = acceptedRequests
                .Select(fr => fr.SenderId == currentUser.Id ? fr.Receiver : fr.Sender)
                .DistinctBy(u => u.Id) 
                .ToList();


            return View(friends);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string friendId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var request = await _context.FriendRequests.FirstOrDefaultAsync(fr =>
                ((fr.SenderId == currentUserId && fr.ReceiverId == friendId) ||
                 (fr.SenderId == friendId && fr.ReceiverId == currentUserId)) &&
                 fr.IsAccepted);

            if (request != null)
            {
                _context.FriendRequests.Remove(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("FriendsList");
        }

        [HttpPost]
        public async Task<IActionResult> CancelRequest(string receiverId)
        {
            var senderId = _userManager.GetUserId(User);

            var request = await _context.FriendRequests.FirstOrDefaultAsync(fr =>
                fr.SenderId == senderId && fr.ReceiverId == receiverId && !fr.IsAccepted);

            if (request != null)
            {
                _context.FriendRequests.Remove(request);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("ViewProfile", "Profile", new { id = receiverId });
        }

    }
}