namespace MobileWebApi.Models
{
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
        public DateTime? TrackingDateTime { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
