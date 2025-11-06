using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proiect.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string Text { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? UserId { get; set; }

        [ValidateNever]
        public ApplicationUser? User { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();


    }
}