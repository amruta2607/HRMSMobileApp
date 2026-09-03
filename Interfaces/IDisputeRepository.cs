using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IDisputeRepository
    {
        Task<IEnumerable<DisputeCategory>> GetDisputeCategoriesAsync();
        Task<Employee?> GetEmployeeByIdAsync(int employeeId);
        Task<EmployeeDispute?> GetExistingDisputeAsync(int employeeId, int disputeCategoryId, DateTime disputeDate);
        Task<EmployeeDispute?> GetEmployeeDisputeByIdAsync(int disputeId, int tenantId);
        Task<int> InsertDisputeAsync(EmployeeDispute dispute);

        /// <summary>
        /// On final approval: apply punch correction or create Punch for Attendance Not Marked,
        /// then set EmployeeDispute.Status = Approved in one transaction.
        /// </summary>
        Task<(bool Success, string Message)> ApproveDisputeAndApplyPunchCorrectionAsync(
            int disputeId,
            int tenantId,
            int updateUserId);

        /// <summary>
        /// On rejection: set EmployeeDispute.Status = Rejected (no punch changes).
        /// </summary>
        Task<(bool Success, string Message)> RejectDisputeAsync(
            int disputeId,
            int tenantId);
    }
}
