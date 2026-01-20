namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for fetching attendance report
    /// </summary>
    public class AttendanceReportRequest
    {
        public int? Id { get; set; }
        public int? BranchId { get; set; }
        public bool Daily { get; set; } = false;
        public bool Monthly { get; set; } = true;
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? CalendarDate { get; set; }
        public int? UserId { get; set; }
        
        /// <summary>
        /// EmployeeId - Internal use only, resolved from UserId by service layer
        /// Not exposed in API requests - clients should use UserId instead
        /// </summary>
        public int? EmployeeId { get; set; }
        
        /// <summary>
        /// DepartmentId - for filtering by department
        /// </summary>
        public int? DepartmentId { get; set; }
        
        /// <summary>
        /// Organization ID (TenantId - foreign key to Tenant table)
        /// </summary>
        public int? organization { get; set; }
    }
}

