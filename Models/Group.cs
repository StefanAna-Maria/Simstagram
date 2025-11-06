using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace proiect.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public string? CreatorId { get; set; } = null!;
        public ApplicationUser? Creator { get; set; } = null!;

        public List<GroupMember> Members { get; set; } = new();
        public List<GroupMessage> Messages { get; set; } = new();

        public ICollection<GroupJoinRequest> PendingRequests { get; set; } = new List<GroupJoinRequest>();
    }



}