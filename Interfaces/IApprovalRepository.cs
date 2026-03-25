using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IApprovalRepository
    {
        // Event operations
        Task<int> InsertEventAsync(int userId, int eventTypeId, string eventData, string state, string status, int tenantId, int insertUserId);
        Task<Event?> GetEventByIdAsync(int eventId, int tenantId);
        Task<int> GetEventTypeIdAsync(string eventName, int tenantId);
        Task<EventType?> GetEventTypeByIdAsync(int eventTypeId, int tenantId);
        Task<bool> IsEventTypeActiveAsync(int eventTypeId, int tenantId);
        Task<bool> UpdateEventStatusAsync(int eventId, string state, string status, int updateUserId, int tenantId);
        
        // Approval Stage operations
        Task<string?> GetFirstLevelNameAsync(int eventTypeId, int tenantId);
        Task<ApprovalStage?> GetApprovalStageByLevelNameAsync(int eventTypeId, string levelName, int tenantId);
        Task<bool> IsApprovalStageActiveAsync(int stageId, int tenantId);
        
        // Approver operations
        Task<IEnumerable<ApproverInfo>> GetApproversForStageAsync(int stageId, int? workRoleId, string explicitUserIds, int tenantId);
        Task<int> GetUserIdByEmployeeIdAsync(int employeeId, int tenantId);
        Task<IEnumerable<string>> GetEmployeeNamesByUserIdsAsync(IEnumerable<int> userIds, int tenantId);
        
        // Approval operations
        Task<int> InsertApprovalAsync(int eventId, int stageId, int approverId, int insertUserId, int tenantId);
        Task<bool> UpdateApprovalStatusAsync(int approvalId, string status, string? comments, int updateUserId);
        Task<IEnumerable<Approval>> GetApprovalsByEventIdAsync(int eventId, int tenantId);
        
        // Screen Notification operations
        Task<int> InsertScreenNotificationAsync(int userId, int? eventId, string title, string message, int tenantId, int insertUserId);
        Task<int> MarkScreenNotificationsReadByLeaveRequestIdAsync(int leaveRequestId, int tenantId, int updateUserId);
        
        // Email operations
        Task<int> InsertEmailNotificationAsync(string toEmail, string subject, string body, int tenantId, int insertUserId);
        Task<EmailTemplate?> GetEmailTemplateAsync(string eventName, string actionType, int tenantId);
        
        // Notification Template operations
        Task<NotificationTemplate?> GetNotificationTemplateAsync(string templateName, string templateType, string actionType, int tenantId);
        
        // Tenant operations
        Task<string?> GetTenantNameAsync(int tenantId);
        
        // Employee operations
        Task<Employee?> GetEmployeeByUserIdAsync(int userId);
        
        // Event Details extraction
        Task<EventDetails> GetEventDetailsAsync(int eventId, int tenantId);
        
        // Payroll operations
        Task UpdatePayrollApprovalStatusAsync(int payrollId, bool isApproved, int tenantId);
    }

    public class EventDetails
    {
        public string PayrollMonthYear { get; set; } = "";
        public string LeaveDates { get; set; } = "";
        public string ReimbursementDates { get; set; } = "";
        public string ResignationDates { get; set; } = "";
        public string OvertimeDates { get; set; } = "";
    }
}

