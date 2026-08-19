namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Represents a single AssetHistory row returned by the timeline query.
    /// </summary>
    public class AssetTimelineResponse
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public int TenantId { get; set; }
        public string SourceTable { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
        public string? Description { get; set; }
        public int? ActionByUserId { get; set; }
    }
}
