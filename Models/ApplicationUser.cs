using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;



namespace proiect.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Display Name is required")]
        public string? DisplayName { get; set; }
        public string? ProfilePicturePath { get; set; }

        [NotMapped]
        public IFormFile? ProfilePictureFile { get; set; }


        public bool IsProfilePublic { get; set; }

        public ICollection<Post> Posts { get; set; } = new List<Post>();

        public ICollection<Album> Albums { get; set; } = new List<Album>();

    }
}