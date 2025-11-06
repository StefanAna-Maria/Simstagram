using proiect.Models;

namespace proiect.ViewModels
{
    public class GroupJoinRequestViewModel
    {
        public int RequestId { get; set; }
        public string RequesterId { get; set; }
        public string RequesterUserName { get; set; }
        public RequestStatus Status { get; set; }
        public DateTime RequestDate { get; set; }
    }
}