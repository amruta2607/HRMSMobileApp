namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for Attendance Overview API
    /// </summary>
    public class AttendanceOverviewRequest
    {
        public int UserId { get; set; }
        
        /// <summary>
        /// Organization ID (TenantId - foreign key to Tenant table)
        /// </summary>
        public int organisationId { get; set; }
        
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}

