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
        /// Insert the initial approval stage for an event
        /// </summary>
        Task InsertInitialApprovalStageAsync(int eventId, int eventTypeId, int userId, int tenantId);
    }
}

