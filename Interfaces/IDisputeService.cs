using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IDisputeService
    {
        Task<DisputeCategoryResponse> GetDisputeCategoriesAsync();

        /// <summary>
        /// Submits a dispute for the authenticated user and tenant.
        /// EmployeeId is resolved from <paramref name="userId"/>; TenantId comes from <paramref name="tenantId"/>.
        /// </summary>
        Task<DisputeSubmitResponse> SubmitDisputeAsync(DisputeSubmitRequest request, int userId, int tenantId);

        /// <summary>
        /// Final approval: marks EmployeeDispute Approved and applies punch correction when applicable.
        /// </summary>
        Task<(bool Success, string Message)> ApproveDisputeAsync(int disputeId, int tenantId, int updateUserId);

        /// <summary>
        /// Rejection: marks EmployeeDispute Rejected (no punch changes).
        /// </summary>
        Task<(bool Success, string Message)> RejectDisputeAsync(int disputeId, int tenantId);
    }
}
