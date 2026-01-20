using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IAlertRepository
    {
        Task<Alert?> GetAlertByIdAsync(int id);
        Task<IEnumerable<Alert>> GetAlertsByUserIdAsync(int userId, bool? isRead = null, bool? isActive = null);
        Task<IEnumerable<Alert>> GetAlertsByOrganisationIdAsync(int organisationId, bool? isRead = null, bool? isActive = null);
        Task<IEnumerable<Alert>> GetAlertsAsync(GetAlertsRequest request);
        Task<int> GetUnreadCountAsync(int userId);
        Task<int> CreateAlertAsync(CreateAlertRequest request);
        Task<bool> UpdateAlertAsync(UpdateAlertRequest request);
        Task<bool> MarkAsReadAsync(int id, int? updateUserId);
        Task<bool> MarkAllAsReadAsync(int userId, int? updateUserId);
        Task<bool> DeleteAlertAsync(int id);
        Task<bool> DeactivateAlertAsync(int id, int? updateUserId);
        Task<bool> ApproveAlertAsync(int id, int? updateUserId);
        Task<bool> RejectAlertAsync(int id, int? updateUserId, string? reason);
    }
}

