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
        /// When <paramref name="assignedApproverUserId"/> is provided (Regularization), that user is the sole first-level approver.
        /// For LeaveRequest, <paramref name="requesterEmployeeId"/> (LeaveRequest.EmployeeId) resolves the reporting manager via SupervisorId.
        /// Otherwise approvers are resolved from the stage WorkRole / ExplicitUserIds.
        /// </summary>
        Task InsertInitialApprovalStageAsync(
            int eventId, int eventTypeId, int userId, int tenantId, string eventName, int? assignedApproverUserId = null, int? requesterEmployeeId = null);
    }
}
