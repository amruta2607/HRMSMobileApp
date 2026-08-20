namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for creating a leave request from Ordinet mobile application
    /// Maps mobile app fields to Altroz HRMS LeaveRequest table
    /// </summary>
    public class LeaveRequestCreateRequest
    {
        /// <summary>
        /// Organization ID (TenantId - foreign key to Tenant table)
        /// </summary>
        public int? organization { get; set; }

        /// <summary>
        /// Leave Type ID (foreign key to LeaveTypes table)
        /// </summary>
        public int leave_type { get; set; }

        /// <summary>
        /// Start date of leave (maps to FromDate)
        /// </summary>
        public DateTime startdate { get; set; }

        /// <summary>
        /// End date of leave (maps to ToDate)
        /// </summary>
        public DateTime enddate { get; set; }

        /// <summary>
        /// Whether this is a half day leave
        /// </summary>
        public bool is_half_day { get; set; }

        /// <summary>
        /// Duration of leave in days
        /// </summary>
        public decimal duration { get; set; }

        /// <summary>
        /// Reason for leave (maps to Description)
        /// </summary>
        public string? reason { get; set; }

        /// <summary>
        /// Attachment file path or base64 content (optional)
        /// </summary>
        public string? attachment { get; set; }

        /// <summary>
        /// User ID from the mobile app (will be resolved to EmployeeId)
        /// </summary>
        public int user { get; set; }
    }
        
}

