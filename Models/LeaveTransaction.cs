namespace MobileWebApi.Models
{
    /// <summary>
    /// Represents the LeaveTransaction table in Altroz HRMS database
    /// </summary>
    public class LeaveTransaction
    {
        public int Id { get; set; }
        /// <summary>
        /// Leave Transaction Type: 1 = AddLeave, 2 = DeductLeave
        /// </summary>
        public int? LeaveTransactionType { get; set; }
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public string? Description { get; set; }
        public decimal LeaveBalance { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? InsertDate { get; set; }
        public int? InsertUserId { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateUserId { get; set; }
        public bool IsActive { get; set; }
        
        /// <summary>
        /// OrganisationId - maps to TenantId column in database
        /// </summary>
        public int OrganisationId { get; set; }
    }
}

