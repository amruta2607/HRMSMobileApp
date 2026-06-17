using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ITenantWeekOffService
    {
        Task<TenantWeekOffResponseDto?> GetTenantWeekOffDaysAsync(int tenantId);
    }
}
