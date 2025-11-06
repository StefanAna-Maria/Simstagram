using System;
using System.ComponentModel.DataAnnotations;

namespace proiect.Models
{
    public class PhotoComment
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;

        public int PhotoId { get; set; }
        public Photo Photo { get; set; }

        public string AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
    }
}