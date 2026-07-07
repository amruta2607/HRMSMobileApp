namespace MobileWebApi.Models
{
    using System.Text.Json.Serialization;
    using MobileWebApi.Helper;

    /// <summary>
    /// Request model for recording GPS location from the mobile application.
    /// </summary>
    public class LocationTrackingRequest
    {
        public int user_id { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        [JsonConverter(typeof(LocationTrackingTimestampJsonConverter))]
        public DateTime timestamp { get; set; }
        public string? location_from { get; set; }
    }
}
