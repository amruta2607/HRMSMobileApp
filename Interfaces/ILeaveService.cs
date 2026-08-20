using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ILeaveService
    {
        /// <summary>
        /// Create a new leave request from mobile app
        /// </summary>
        Task<LeaveRequestResponse> CreateLeaveRequestAsync(LeaveRequestCreateRequest request);
        
        /// <summary>
        /// Get leave requests with filters from mobile app
        /// </summary>
        Task<LeaveRequestResponse> GetLeaveRequestsAsync(LeaveRequestGetRequest request);
        
        /// <summary>
        /// Get leave request by ID
        /// </summary>
        Task<LeaveRequestResponse> GetLeaveRequestByIdAsync(int id);
        
        /// <summary>
        /// Approve a leave request
        /// </summary>
        Task<LeaveRequestResponse> ApproveLeaveRequestAsync(int id, int approverUserId);
        
        /// <summary>
        /// Reject a leave request
        /// </summary>
        Task<LeaveRequestResponse> RejectLeaveRequestAsync(int id, int rejecterUserId, string? reason);
        
        /// <summary>
        /// Cancel a leave request
        /// </summary>
        Task<LeaveRequestResponse> CancelLeaveRequestAsync(int id, int userId, string? reason);
        
        /// <summary>
        /// Get leave balance for an employee
        /// </summary>
        Task<LeaveBalanceResponse> GetLeaveBalanceAsync(int userId, int? organization);
		Task<LeaveRequestResponse> WithdrawLeaveRequestAsync(int id, int userId, string? reason);
		Task<LeaveHistoryResponse> GetLeaveHistoryAsync(int userId);
		Task<LeaveHistorySummaryResponse> GetLeaveHistorySummaryAsync(int userId);
	}
}

