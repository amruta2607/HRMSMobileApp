namespace MobileWebApi.Models
{
    using System.Text.Json.Serialization;
    using MobileWebApi.Helper;

    /// <summary>
    /// A single location entry within a batch upload request.
    /// </summary>
    public class LocationTrackingBatchItem
    {
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        [JsonConverter(typeof(LocationTrackingTimestampJsonConverter))]
        public DateTime timestamp { get; set; }
        public string? location_from { get; set; }
    }

    /// <summary>
    /// Request model for batch location upload from the mobile application.
    /// </summary>
    public class LocationTrackingBatchRequest
    {
        public int user_id { get; set; }
        public List<LocationTrackingBatchItem> locations { get; set; } = new();
    }
}
