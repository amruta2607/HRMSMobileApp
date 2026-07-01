namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for recording GPS location from the mobile application.
    /// </summary>
    public class LocationTrackingRequest
    {
        public int userId { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public DateTime trackingDateTime { get; set; }
    }
}
