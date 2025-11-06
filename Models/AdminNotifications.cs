using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proiect.Models
{
    public class AdminNotification
    {
        public int Id { get; set; }

        [Required]
        public string RecipientId { get; set; }

        [ForeignKey("RecipientId")]
        public ApplicationUser Recipient { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}