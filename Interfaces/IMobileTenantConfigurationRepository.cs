using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IMobileTenantConfigurationRepository
    {
        Task<MobileTenantConfiguration?> GetByTenantIdAsync(int tenantId);
    }
}

