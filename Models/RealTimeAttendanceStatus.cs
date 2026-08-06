namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents real-time attendance status for employees
    /// Shows who is currently punched in (no punch out yet)
    /// </summary>
    public class RealTimeAttendanceStatus
    {
        public int EmployeeId { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeNumber { get; set; }
        public string? DepartmentName { get; set; }
        public string? BranchName { get; set; }

        /// <summary>
        /// Punch table primary key.
        /// </summary>
        public int? PunchId { get; set; }

        public DateTime PunchDate { get; set; }
        public DateTime? PunchIn { get; set; }
        public DateTime? PunchOut { get; set; }
        public bool IsPunchedIn => PunchIn.HasValue && !PunchOut.HasValue;
        
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int OrganisationId { get; set; }
    }

    /// <summary>
    /// Response model for real-time attendance status
    /// </summary>
    public class RealTimeAttendanceResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public IEnumerable<RealTimeAttendanceStatus>? Data { get; set; }
        public int TotalPunchedIn { get; set; }
        public int TotalPunchedOut { get; set; }
        public int TotalNotPunched { get; set; }
    }
}

