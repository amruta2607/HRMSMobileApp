using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ILeaveRepository
    {
        // Leave Request operations
        Task<int> CreateLeaveRequestAsync(LeaveRequest leaveRequest);
        Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id);
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsAsync(int? organisationId, int? employeeId, int? leaveTypeId, string? status);
        Task<IEnumerable<LeaveRequest>> GetLeaveRequestsByEmployeeIdAsync(int employeeId);
        Task<bool> UpdateLeaveRequestStatusAsync(int id, int statusId, string statusText, int updateUserId);
        
        // Leave Balance operations
        Task<IEnumerable<LeaveBalance>> GetLeaveBalanceByEmployeeIdAsync(int employeeId);
        Task<LeaveBalance?> GetLeaveBalanceAsync(int employeeId, int leaveTypeId);
        Task<bool> UpdateLeaveBalanceAsync(int employeeId, int leaveTypeId, decimal newBalance, int updateUserId);
        
        // Leave Transaction operations
        Task<int> CreateLeaveTransactionAsync(LeaveTransaction transaction);
        Task<IEnumerable<LeaveTransaction>> GetLeaveTransactionsByEmployeeIdAsync(int employeeId);
        
        // Lookup operations
        Task<int?> GetLeaveTypeIdByNameAsync(string leaveTypeName);
        Task<int?> GetEmployeeIdByUserIdAsync(int userId);
        Task<string?> GenerateLeaveRequestNumberAsync(int organisationId);
    }
}

