namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for attendance summary API
    /// </summary>
    public class AttendanceSummaryResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int OrganizationId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TotalDays { get; set; }
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HolidayDays { get; set; }
        public int WeekendDays { get; set; }
        public double TotalWorkingHours { get; set; }
        public double AverageWorkingHours { get; set; }
        public List<AttendanceSummaryDetail>? AttendanceDetails { get; set; }
    }

    /// <summary>
    /// Attendance detail for each day
    /// </summary>
    public class AttendanceSummaryDetail
    {
        public DateTime Date { get; set; }
        public string? DayName { get; set; }
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
        public double? WorkingHours { get; set; }
        public string? Status { get; set; } // "Present", "Absent", "Leave", "Holiday", "Weekend"
    }
}

