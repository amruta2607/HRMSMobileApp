namespace MobileWebApi.Models
{
    /// <summary>
    /// Request for today's location tracking path.
    /// Bind from query string on GET (e.g. ?UserId=8).
    /// </summary>
    public class TodayLocationTrackingRequest
    {
        public int UserId { get; set; }
    }
}
