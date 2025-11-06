using proiect.Models;
using System.Collections.Generic;

namespace proiect.ViewModels
{
    public class GroupIndexViewModel
    {
        public List<Group> MyGroups { get; set; }
        public List<Group> AllGroups { get; set; }
        public string CurrentUserId { get; set; }
    }
}