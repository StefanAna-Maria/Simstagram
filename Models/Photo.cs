using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace proiect.Models
{
    public class Photo
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public int AlbumId { get; set; }

        [ForeignKey("AlbumId")]
        public Album Album { get; set; }

        public ICollection<PhotoComment> Comments { get; set; } = new List<PhotoComment>();
    }
}