namespace MobileWebApi.Models
{
    /// <summary>
    /// Request for location tracking path on a specified date.
    /// Bind from query string on GET (e.g. ?UserId=8&amp;Date=2026-08-24).
    /// </summary>
    public class ByDateLocationTrackingRequest
    {
        public int UserId { get; set; }

        public DateTime? Date { get; set; }
    }
}
