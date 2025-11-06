using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace proiect.Models
{
    public class GroupCreateViewModel
    {
        [Required]
        public string Name { get; set; }

        public List<string> SelectedUserIds { get; set; } = new List<string>();

        public List<ApplicationUser> AvailableUsers { get; set; } = new List<ApplicationUser>();
    }
}