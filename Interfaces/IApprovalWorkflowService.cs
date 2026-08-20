using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IApprovalWorkflowService
    {
        /// <summary>
        /// Initiates the approval workflow for a leave request
        /// </summary>
        Task<(bool Success, string Message, int EventId)> InitiateLeaveRequestApprovalAsync(LeaveRequest leaveRequest, int userId, int tenantId);

        /// <summary>
        /// Initiates the approval workflow for a regularization (EmployeeDispute) request.
        /// First-level approval is assigned to the employee's reporting manager (<paramref name="managerUserId"/>).
        /// </summary>
        Task<(bool Success, string Message, int EventId)> InitiateRegularizationRequestApprovalAsync(
            EmployeeDispute dispute, int userId, int tenantId, int managerUserId);
        
        /// <summary>
        /// Insert the initial approval stage for an event and notify approvers via the Alert framework.
        /// When <paramref name="assignedApproverUserId"/> is provided, that user is the sole first-level approver
        /// (manager-based routing). Otherwise approvers are resolved from the stage WorkRole / ExplicitUserIds.
        /// </summary>
        Task InsertInitialApprovalStageAsync(
            int eventId, int eventTypeId, int userId, int tenantId, string eventName, int? assignedApproverUserId = null);
    }
}
