using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using proiect.ContextModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace proiect.ViewComponents
{
    public class FriendsListSidebarViewComponent : ViewComponent
    {
        private readonly ApplicationContext _context;

        public FriendsListSidebarViewComponent(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            var friends = await _context.FriendRequests
                .Where(f => f.IsAccepted &&
                            (f.SenderId == userId || f.ReceiverId == userId))
                .Select(f => f.SenderId == userId ? f.Receiver : f.Sender)
                .ToListAsync();


            return View(friends);
        }
    }
}