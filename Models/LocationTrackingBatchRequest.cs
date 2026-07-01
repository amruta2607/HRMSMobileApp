namespace MobileWebApi.Models
{
    /// <summary>
    /// A single location entry within a batch upload request.
    /// </summary>
    public class LocationTrackingBatchItem
    {
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public DateTime trackingDateTime { get; set; }
    }

    /// <summary>
    /// Request model for batch location upload from the mobile application.
    /// </summary>
    public class LocationTrackingBatchRequest
    {
        public int userId { get; set; }
        public List<LocationTrackingBatchItem> locations { get; set; } = new();
    }
}
