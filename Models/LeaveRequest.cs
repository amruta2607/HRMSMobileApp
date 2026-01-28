namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents the LeaveRequest table in Altroz HRMS database
    /// </summary>
    public class LeaveRequest
    {
        public int Id { get; set; }
        public string? Number { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public decimal? LeaveBalance { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal Duration { get; set; }
        public string? Description { get; set; }
        public string? CancellationReason { get; set; }
        public string? CurrentAction { get; set; }
        public int? LeaveRequestStatus { get; set; }
        public int? DelegatedEmployeeId { get; set; }
        
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int? OrganisationId { get; set; }
      
        public DateTime? InsertDate { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
        
        // Navigation properties for display
        public string? EmployeeName { get; set; }
        public string? LeaveTypeName { get; set; }
    }
}

