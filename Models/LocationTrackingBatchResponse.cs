namespace MobileWebApi.Models
{
    using System.Text.Json.Serialization;
    using MobileWebApi.Helper;

    /// <summary>
    /// Response model for batch location upload.
    /// </summary>
    public class LocationTrackingBatchResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalRecords { get; set; }
        public int InsertedRecords { get; set; }
        public int FailedRecords { get; set; }
        public List<LocationTrackingBatchFailedRecord>? FailedRecordDetails { get; set; }
    }

    /// <summary>
    /// Details for a location record that failed per-item validation.
    /// </summary>
    public class LocationTrackingBatchFailedRecord
    {
        [JsonConverter(typeof(NullableLocationTrackingTimestampJsonConverter))]
        public DateTime? timestamp { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
