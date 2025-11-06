using System.Collections.Generic;

namespace proiect.Models
{
    public class ConversationCreateViewModel
    {
        public List<ApplicationUser> Friends { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
    }
}