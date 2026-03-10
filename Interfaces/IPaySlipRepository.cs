using MobileWebApi.Models;
using MobileWebApi.Repositories;

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
		Task<(decimal MyShare, decimal EmployerShare)>GetEmployeeProvidentFundSummaryAsync(int employeeId, int tenantId);
		Task<MonthlyPaymentSummary?> GetMonthlyPaymentSummaryAsync(int employeeId,int tenantId,int month,int year);

		Task<(int Month, int Year)?>GetLatestPayrollPeriodAsync(int employeeId,int tenantId);
		Task<IEnumerable<PaySlipLineItem>> GetPaySlipIncomesAsync(int paySlipId);
		Task<IEnumerable<PaySlipLineItem>> GetPaySlipDeductionsAsync(int paySlipId);
		Task<PaySlipWithWeekOff?> GetPaySlipWithWeekOffAsync(int employeeId, int tenantId, int month, int year);
		Task<IEnumerable<PaySlipMonthItem>> GetPaySlipMonthsByYearAsync(int employeeId, int tenantId, int year);
	}
}
