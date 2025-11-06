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
    public class ConversationController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ConversationController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            var conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt))
                .Where(c => c.User1Id == currentUserId || c.User2Id == currentUserId)
                .ToListAsync();

            return View(conversations);
        }

        public async Task<IActionResult> Chat(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var conversation = await _context.Conversations
                .Include(c => c.Messages.OrderBy(m => m.SentAt))
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conversation == null || (conversation.User1Id != currentUserId && conversation.User2Id != currentUserId))
                return NotFound();

            return View(conversation);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            var currentUserId = _userManager.GetUserId(User);

            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null || (conversation.User1Id != currentUserId && conversation.User2Id != currentUserId))
                return NotFound();

            var receiverId = conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id;

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = currentUserId,
                ReceiverId = receiverId,
                Content = content,
                SentAt = DateTime.Now,
                IsRead = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = conversationId });
        }

        [HttpGet("Conversation/Start/{receiverId}")]
        public async Task<IActionResult> Start(string receiverId)
        {
            var currentUserId = _userManager.GetUserId(User);

            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    (c.User1Id == currentUserId && c.User2Id == receiverId) ||
                    (c.User1Id == receiverId && c.User2Id == currentUserId));

            if (conversation == null)
            {
                conversation = new Conversation
                {
                    User1Id = currentUserId,
                    User2Id = receiverId
                };
                _context.Conversations.Add(conversation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Chat", new { id = conversation.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Start(ConversationCreateViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            var userId = model.ReceiverId;

            var existing = await _context.Conversations.FirstOrDefaultAsync(c =>
                (c.User1Id == currentUserId && c.User2Id == userId) ||
                (c.User1Id == userId && c.User2Id == currentUserId));

            if (existing != null)
            {
                return RedirectToAction("Chat", new { id = existing.Id });
            }

            var newConversation = new Conversation
            {
                User1Id = currentUserId,
                User2Id = userId
            };
            _context.Conversations.Add(newConversation);
            await _context.SaveChangesAsync();

            var message = new Message
            {
                ConversationId = newConversation.Id,
                SenderId = currentUserId,
                ReceiverId = userId,
                Content = model.Message,
                SentAt = DateTime.Now,
                IsRead = false
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = newConversation.Id });
        }

        private async Task<int> GetUnreadMessageCount(string userId)
        {
            return await _context.Messages
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }

        [HttpGet]
        public async Task<IActionResult> Start()
        {
            var currentUserId = _userManager.GetUserId(User);

            var friends = await _context.FriendRequests
                .Where(fr => fr.IsAccepted && (fr.SenderId == currentUserId || fr.ReceiverId == currentUserId))
                .Select(fr => fr.SenderId == currentUserId ? fr.Receiver : fr.Sender)
                .ToListAsync();

            var model = new ConversationCreateViewModel
            {
                Friends = friends
            };

            return View(model);
        }

    }
}