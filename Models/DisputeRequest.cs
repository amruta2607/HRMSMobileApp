namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for submitting a dispute.
    /// System fields (Id, TenantId, CreatedOn, Status) are set by the server.
    /// </summary>
    public class DisputeSubmitRequest
    {
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public int DisputeCategoryId { get; set; }

        /// <summary>
        /// Dispute date/time.
        /// Format: yyyy-MM-ddTHH:mm:ss
        /// Example: 2026-07-22T18:00:00
        /// </summary>
        public DateTime DisputeDate { get; set; }

        public string Description { get; set; } = string.Empty;
        public int? PunchId { get; set; }

        /// <summary>
        /// Requested punch-in time for the disputed attendance.
        /// Format: yyyy-MM-ddTHH:mm:ss
        /// Example: 2026-07-22T09:00:00
        /// </summary>
        public DateTime? RequestedPunchInTime { get; set; }

        /// <summary>
        /// Requested punch-out time for the disputed attendance.
        /// Format: yyyy-MM-ddTHH:mm:ss
        /// Example: 2026-07-22T18:00:00
        /// </summary>
        public DateTime? RequestedPunchOutTime { get; set; }
    }
}
