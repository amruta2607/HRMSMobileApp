namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// A single punch in/out event in the tracking timeline.
    /// </summary>
    public class PunchTrackingTimelineItemDto
    {
        public int Id { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string? Source { get; set; }
        public string? Coordinate { get; set; }
        public string? Address { get; set; }
		public string? PunchInImage { get; set; }
		public string? PunchOutImage { get; set; }
        public bool? Manual { get; set; }
        public string? Remarks { get; set; }
    }
}
