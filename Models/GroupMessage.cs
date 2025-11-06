using System;

namespace proiect.Models
{
    public class GroupMessage
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        public string Content { get; set; }

        public DateTime SentAt { get; set; }
    }
}