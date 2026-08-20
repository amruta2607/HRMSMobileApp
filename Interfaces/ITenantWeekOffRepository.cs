using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ITenantWeekOffRepository
    {
        Task<int?> GetTenantConfigurationIdByTenantIdAsync(int tenantId);
        Task<List<TenantWeekOffDayDto>> GetWeekOffDaysByTenantIdAsync(int tenantId);
        Task<List<DayOfWeek>> GetTenantWeeklyOffDaysAsync(int tenantId);
    }
}
