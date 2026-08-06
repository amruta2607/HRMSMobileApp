namespace MobileWebApi.Models
{
    /// <summary>
    /// Request model for submitting a dispute.
    /// System fields (Id, EmployeeId, TenantId, UserId, CreatedOn, Status) are set by the server
    /// from the authenticated user context — they must not be sent by the client.
    /// </summary>
    public class DisputeSubmitRequest
    {
        public int DisputeCategoryId { get; set; }

        /// <summary>
        /// Dispute date/time.
        /// Format: yyyy-MM-ddTHH:mm:ss
        /// Example: 2026-07-22T18:00:00
        /// </summary>
        public DateTime DisputeDate { get; set; }

        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Optional punch reference. Defaults to 0 when not provided.
        /// </summary>
        public int PunchId { get; set; }

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
