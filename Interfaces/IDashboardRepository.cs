using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IDashboardRepository
    {
        Task<IReadOnlyList<DashboardBirthdayDto>> GetUpcomingBirthdaysAsync(int tenantId, DateTime today, DateTime endDate);
        Task<IReadOnlyList<DashboardWorkAnniversaryDto>> GetUpcomingWorkAnniversariesAsync(int tenantId, DateTime today, DateTime endDate);
        Task<IReadOnlyList<DashboardAwardDto>> GetUpcomingAwardsAsync(int tenantId, DateTime today, DateTime endDate);
    }
}
