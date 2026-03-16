using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IAlertService
    {
        Task<AlertResponse> GetAlertByIdAsync(int id);
        Task<AlertListResponse> GetAlertsByUserIdAsync(int userId, bool? isRead = null, bool? isActive = null);
        Task<AlertListResponse> GetAlertsByOrganisationIdAsync(int organisationId, bool? isRead = null, bool? isActive = null);
        Task<AlertListResponse> GetAlertsAsync(GetAlertsRequest request);
	        Task<AlertCountResponse> GetUnreadAlertCountByUserIdAsync(int userId);
        Task<AlertResponse> CreateAlertAsync(CreateAlertRequest request);
        Task<AlertResponse> UpdateAlertAsync(UpdateAlertRequest request);
        Task<AlertResponse> MarkAsReadAsync(int id, int? updateUserId);
        Task<AlertResponse> MarkAllAsReadAsync(int userId, int? updateUserId);
        Task<AlertResponse> DeleteAlertAsync(int id);
        Task<AlertResponse> DeactivateAlertAsync(int id, int? updateUserId);
        Task<AlertResponse> ApproveAlertAsync(int id, int? updateUserId);
        Task<AlertResponse> RejectAlertAsync(int id, int? updateUserId, string? reason);
        Task<AlertResponse> SendApprovalNotificationAsync(SendApprovalNotificationRequest request, int organizationId, int approverUserId);
        Task<AlertResponse> SendRejectionNotificationAsync(SendRejectionNotificationRequest request, int organizationId, int rejecterUserId);
        Task<AlertResponse> ApproveRequestFromAlertAsync(ApproveRequestFromAlertRequest request, int approverUserId);
        Task<AlertResponse> RejectRequestFromAlertAsync(RejectRequestFromAlertRequest request, int rejecterUserId);
    }
}

