using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IDashboardService
    {
        Task<IReadOnlyList<DashboardBirthdayDto>> GetUpcomingBirthdaysAsync(int tenantId);
        Task<IReadOnlyList<DashboardWorkAnniversaryDto>> GetUpcomingWorkAnniversariesAsync(int tenantId);
        Task<IReadOnlyList<DashboardAwardDto>> GetUpcomingAwardsAsync(int tenantId);
    }
}
