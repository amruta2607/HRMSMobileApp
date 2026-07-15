namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Response model for the punch tracking timeline API.
    /// </summary>
    public class PunchTrackingTimelineResponse
    {
        public int PunchId { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime PunchDate { get; set; }
        public string? FirstPunchIn { get; set; }
        public string? LastPunchOut { get; set; }
        public int TotalEntries { get; set; }
        public List<PunchTrackingTimelineItemDto> Timeline { get; set; } = new();
    }
}
