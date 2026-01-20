namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for Attendance Overview API
    /// </summary>
    public class AttendanceOverviewResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public AttendanceOverviewData? Data { get; set; }
    }

    /// <summary>
    /// Attendance Overview Data
    /// </summary>
    public class AttendanceOverviewData
    {
        public string Week { get; set; } = string.Empty; // Format: "FromDate – ToDate"
        public double ExpectedHours { get; set; }
        public double ActualHours { get; set; }
        public double ShortfallHours { get; set; }
    }
}

