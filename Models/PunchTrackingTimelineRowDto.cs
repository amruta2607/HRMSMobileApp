namespace MobileWebApi.Models
{
    /// <summary>
    /// Internal DTO for mapping GetPunchTrackingTimeline query results.
    /// </summary>
    internal class PunchTrackingTimelineRowDto
    {
        public int Id { get; set; }
        public int PunchId { get; set; }
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public DateTime PunchDate { get; set; }
        public string Direction { get; set; } = string.Empty;
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
        public DateTime? PunchTime { get; set; }
        public string? Source { get; set; }
        public string? Coordinate { get; set; }
        public string? Address { get; set; }
		public string? PunchInImage { get; set; }
		public string? PunchOutImage { get; set; }
		public bool? Manual { get; set; }
        public string? Remarks { get; set; }
    }
}
