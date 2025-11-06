using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proiect.Models
{
    public class Album
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    }
}