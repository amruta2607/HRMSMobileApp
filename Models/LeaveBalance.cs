namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents the LeaveBalance table in Altroz HRMS database
    /// </summary>
    public class LeaveBalance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public int TotalBalance { get; set; }
        public int RemainingBalance { get; set; }
        public string? Description { get; set; }
        public DateTime? InsertDate { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
        
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int OrganisationId { get; set; }
        
        public string? LeaveTypeName { get; set; }
    }
}

