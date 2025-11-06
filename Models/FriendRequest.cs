using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proiect.Models
{
    public class FriendRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string ReceiverId { get; set; }

        public bool IsAccepted { get; set; } = false;

        [ForeignKey("SenderId")]
        public ApplicationUser Sender { get; set; }

        [ForeignKey("ReceiverId")]
        public ApplicationUser Receiver { get; set; }
    }
}