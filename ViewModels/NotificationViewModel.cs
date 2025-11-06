using proiect.Models;
using System.Collections.Generic;

namespace proiect.ViewModels
{
    public class NotificationViewModel
    {
        public List<Comment> PendingPostComments { get; set; } = new();
        public List<PhotoComment> PendingPhotoComments { get; set; } = new();
        public List<AdminNotification> AdminWarnings { get; set; }

    }
}