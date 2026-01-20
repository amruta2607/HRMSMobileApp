namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for attendance report API
    /// </summary>
    public class AttendanceReportResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<AttendanceReport>? Data { get; set; }
        public int TotalRecords { get; set; }
        public int? TotalWorkingDays { get; set; }
        public double? TotalWorkingHours { get; set; }
    }
}

