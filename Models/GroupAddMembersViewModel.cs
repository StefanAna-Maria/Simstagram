namespace proiect.Models
{
    public class GroupAddMembersViewModel
    {
        public int GroupId { get; set; }
        public List<ApplicationUser> AvailableUsers { get; set; } = new();
        public List<string> SelectedUserIds { get; set; } = new();
    }
}