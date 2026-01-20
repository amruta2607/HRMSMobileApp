using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IPaySlipRepository
    {
        /// <summary>
        /// Get list of pay slips for an employee with optional filters (filtered by tenant)
        /// </summary>
        Task<IEnumerable<PaySlip>> GetPaySlipsAsync(int employeeId, int tenantId, int? year = null, int? month = null);
        
        /// <summary>
        /// Get a specific pay slip by ID (filtered by tenant)
        /// </summary>
        Task<PaySlip?> GetPaySlipByIdAsync(int id, int tenantId);
        
        /// <summary>
        /// Get pay slip by employee, month and year (filtered by tenant)
        /// </summary>
        Task<PaySlip?> GetPaySlipByEmployeeMonthYearAsync(int employeeId, int tenantId, int month, int year);
        
        /// <summary>
        /// Get employee ID and TenantId by user ID
        /// </summary>
        Task<(int? EmployeeId, int? TenantId)> GetEmployeeIdAndTenantByUserIdAsync(int userId);
    }
}
