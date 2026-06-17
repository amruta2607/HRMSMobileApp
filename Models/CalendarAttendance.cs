namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for calendar-based attendance
    /// </summary>
    public class CalendarAttendanceRequest
    {
        public int UserId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
    }

    /// <summary>
    /// Represents attendance data for a single day in the calendar
    /// </summary>
    public class CalendarDayAttendance
    {
        public DateTime Date { get; set; }
        public int Day { get; set; }
        public string? DayName { get; set; }
        public bool IsWeekend { get; set; }
        public bool IsPresent { get; set; }
        public bool IsAbsent { get; set; }
        public bool IsLeave { get; set; }
        public bool IsHoliday { get; set; }
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
        public double? WorkingHours { get; set; }
        public string? Status { get; set; } // "Present", "Absent", "Leave", "Holiday", "Week Off", "Future"
        public string? Remarks { get; set; }
        public string? InSource { get; set; }
        public string? OutSource { get; set; }
        public string? CoordinateIn { get; set; }
        public string? CoordinateOut { get; set; }
        public string? LinkIn { get; set; }
        public string? LinkOut { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// Response model for calendar-based attendance
    /// </summary>
    public class CalendarAttendanceResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public int Month { get; set; }
        public string? MonthName { get; set; }
        public int Year { get; set; }
        public int TotalDays { get; set; }
        public int WorkingDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public int LeaveDays { get; set; }
        public int HolidayDays { get; set; }
        public int WeekendDays { get; set; }
        public double TotalWorkingHours { get; set; }
        public List<CalendarDayAttendance>? CalendarData { get; set; }
    }
}

