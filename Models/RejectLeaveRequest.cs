namespace MobileWebApi.Models
{
    public class RejectLeaveRequest
    {
        public int Id { get; set; }
        public int RejecterUserId { get; set; }
        public string? Reason { get; set; }
    }
}

