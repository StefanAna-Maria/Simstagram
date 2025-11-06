using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proiect.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; } = false;

        [Required]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public ApplicationUser Author { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post Post { get; set; }
    }
}