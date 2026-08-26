namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for getting leave requests from Ordinet mobile application
    /// </summary>
    public class LeaveRequestGetRequest
    {
        /// <summary>
        /// Organization ID (TenantId - foreign key to Tenant table)
        /// </summary>
        public int? organization { get; set; }

        /// <summary>
        /// Leave Type ID filter (foreign key to LeaveTypes table)
        /// </summary>
        public int? leave_type { get; set; }

        /// <summary>
        /// Status filter: Pending, Approved, Submit, etc.
        /// </summary>
       // public string? status { get; set; }

        /// <summary>
        /// User ID to filter leave requests for specific user
        /// </summary>
        public int? user { get; set; }
		public string? HalfDayType { get; set; }


	}
}

