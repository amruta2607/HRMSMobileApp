namespace MobileWebApi.Models
{
    /// <summary>
    /// Entity model mapped to the LocationTrackingIssue table.
    /// </summary>
    public class LocationTrackingIssue
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TenantId { get; set; }
        public string IssueType { get; set; } = string.Empty;
        public string IssueDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public decimal LastKnownLatitude { get; set; }
        public decimal LastKnownLongitude { get; set; }
        public string? DeviceId { get; set; }
        public int InsertUserId { get; set; }
        public DateTime InsertDate { get; set; }
        public int UpdateUserId { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
