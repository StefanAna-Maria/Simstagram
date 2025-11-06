using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proiect.Models
{
    public class GroupJoinRequest
    {
        public int Id { get; set; }

        [Required]
        public string RequesterId { get; set; }
        [ForeignKey("RequesterId")]
        public ApplicationUser Requester { get; set; }

        [Required]
        public int GroupId { get; set; }
        [ForeignKey("GroupId")]
        public Group Group { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }
}