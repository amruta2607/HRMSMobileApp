namespace MobileWebApi.Models.Requests
{
    /// <summary>
    /// Request for today's location tracking path. UserId is resolved to EmployeeId server-side.
    /// </summary>
    public class TodayLocationTrackingRequest
    {
        public int UserId { get; set; }
    }
}
