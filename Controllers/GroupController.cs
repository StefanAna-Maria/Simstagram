using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using proiect.Models;
using proiect.ViewModels;
using System.Security.Claims;

namespace proiect.Controllers
{
    [Authorize]
    public class GroupController : Controller
    {
        private readonly ApplicationContext _context;

        private readonly UserManager<ApplicationUser> _userManager;


        public GroupController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var myGroups = await _context.GroupMembers
                .Include(gm => gm.Group)
                    .ThenInclude(g => g.Members)
                        .ThenInclude(m => m.User)
                .Where(gm => gm.UserId == userId)
                .Select(gm => gm.Group)
                .ToListAsync();

            var allGroups = await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .ToListAsync();

            var model = new GroupIndexViewModel
            {
                MyGroups = myGroups,
                AllGroups = allGroups,
                CurrentUserId = userId
            };

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Chat(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == id && gm.UserId == userId);

            if (!isMember)
                return Forbid();

            var group = await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User) 
                .Include(g => g.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            ViewBag.GroupId = id;
            ViewBag.CurrentUserId = userId;

            return View(group);
        }




        [HttpPost]
        public async Task<IActionResult> SendMessage(int groupId, string content)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isMember = await _context.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
            if (!isMember)
                return Forbid();

            var message = new GroupMessage
            {
                GroupId = groupId,
                SenderId = userId,
                Content = content,
                SentAt = DateTime.Now
            };

            _context.GroupMessages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = groupId });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var friends = await _context.FriendRequests
                .Where(fr => fr.IsAccepted && (fr.SenderId == currentUserId || fr.ReceiverId == currentUserId))
                .Select(fr => fr.SenderId == currentUserId ? fr.Receiver : fr.Sender)
                .ToListAsync();

            var model = new GroupCreateViewModel
            {
                AvailableUsers = friends
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var group = new Group
            {
                Name = model.Name,
                CreatedAt = DateTime.Now,
                CreatorId = currentUserId
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = group.Id,
                UserId = currentUserId
            });

            foreach (var friendId in model.SelectedUserIds)
            {
                _context.GroupMembers.Add(new GroupMember
                {
                    GroupId = group.Id,
                    UserId = friendId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> View(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var group = await _context.Groups
                .Include(g => g.Members)
                    .ThenInclude(m => m.User)
                .Include(g => g.PendingRequests)  
                    .ThenInclude(r => r.Requester) 
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            ViewBag.CurrentUserId = currentUserId;

            return View(group);
        }




        [HttpGet]
        public async Task<IActionResult> AddMembers(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var group = await _context.Groups
                .Include(g => g.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null)
                return NotFound();

            var friends = await _context.FriendRequests
                .Where(fr => fr.IsAccepted && (fr.SenderId == currentUserId || fr.ReceiverId == currentUserId))
                .Select(fr => fr.SenderId == currentUserId ? fr.Receiver : fr.Sender)
                .ToListAsync();

            var existingIds = group.Members.Select(m => m.UserId).ToList();
            var available = friends.Where(f => !existingIds.Contains(f.Id)).ToList();

            var model = new GroupAddMembersViewModel
            {
                GroupId = id,
                AvailableUsers = available
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMembers(GroupAddMembersViewModel model)
        {
            if (model.SelectedUserIds == null || model.SelectedUserIds.Count == 0)
                return RedirectToAction("View", new { id = model.GroupId });

            foreach (var userId in model.SelectedUserIds)
            {
                _context.GroupMembers.Add(new GroupMember
                {
                    GroupId = model.GroupId,
                    UserId = userId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("View", new { id = model.GroupId });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RemoveMember(int groupId, string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId && g.CreatorId == currentUserId);

            if (group == null ||  group.CreatorId != currentUserId)
                return Forbid();

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (member != null)
            {
                _context.GroupMembers.Remove(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("View", new { id = groupId });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var group = await _context.Groups
                .Include(g => g.Messages)
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId && g.CreatorId == currentUserId);

            if (group == null)
                return Forbid();

            _context.GroupMessages.RemoveRange(group.Messages);

            _context.GroupMembers.RemoveRange(group.Members);

            _context.Groups.Remove(group);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RequestToJoin(int groupId)
        {
            var userId = _userManager.GetUserId(User);
            var existingRequest = await _context.GroupJoinRequests
                .FirstOrDefaultAsync(r => r.GroupId == groupId && r.RequesterId == userId);

            var isAlreadyMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (existingRequest != null || isAlreadyMember)
            {
                return RedirectToAction("View", new { id = groupId });
            }

            var request = new GroupJoinRequest
            {
                GroupId = groupId,
                RequesterId = userId,
                Status = RequestStatus.Pending,
                RequestDate = DateTime.UtcNow
            };

            _context.GroupJoinRequests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("View", new { id = groupId });
        }

        [Authorize]
        public async Task<IActionResult> ManageRequests(int groupId)
        {
            var userId = _userManager.GetUserId(User);
            var group = await _context.Groups.FindAsync(groupId);

            if (group == null || group.CreatorId != userId)
                return NotFound();

            var requests = await _context.GroupJoinRequests
                .Where(r => r.GroupId == groupId && r.Status == RequestStatus.Pending)
                .Select(r => new GroupJoinRequestViewModel
                {
                    RequestId = r.Id,
                    RequesterId = r.RequesterId,
                    RequesterUserName = r.Requester.UserName,
                    Status = r.Status,
                    RequestDate = r.RequestDate
                })
                .ToListAsync();

            ViewBag.GroupId = groupId;
            return View(requests);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RespondToRequest(int requestId, bool accept)
        {
            var request = await _context.GroupJoinRequests
                .Include(r => r.Group)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (request.Group.CreatorId != userId)
                return Forbid();

            if (accept)
            {
                _context.GroupMembers.Add(new GroupMember
                {
                    GroupId = request.GroupId,
                    UserId = request.RequesterId
                });
                request.Status = RequestStatus.Approved;
            }
            else
            {
                request.Status = RequestStatus.Rejected;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageRequests", new { groupId = request.GroupId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var userId = _userManager.GetUserId(User);

            var group = await _context.Groups
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return NotFound();

            if (group.CreatorId == userId)
                return Forbid();

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId);

            if (member != null)
            {
                _context.GroupMembers.Remove(member);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



    }
}