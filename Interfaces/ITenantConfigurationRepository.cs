using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
	public interface ITenantConfigurationRepository
	{
		Task<TenantConfiguration> GetByTenantIdAsync(int tenantId);
	}

}
