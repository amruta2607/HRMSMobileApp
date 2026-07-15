using System.Text.Json.Serialization;
using MobileWebApi.Helper;

namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for logging a location tracking violation from the mobile application.
    /// </summary>
    public class LocationTrackingIssueRequest
    {
        public int user_id { get; set; }
        public string issue_type { get; set; } = string.Empty;
        public string issue_description { get; set; } = string.Empty;

        [JsonConverter(typeof(LocationTrackingTimestampJsonConverter))]
        public DateTime timestamp { get; set; }

        public decimal last_known_latitude { get; set; }
        public decimal last_known_longitude { get; set; }
        public string? device_id { get; set; }
    }
}
