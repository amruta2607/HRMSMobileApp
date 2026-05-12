using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
	public interface ITenantConfigurationRepository
	{
		Task<TenantConfiguration> GetByTenantIdAsync(int tenantId);

		/// <summary>
		/// Loads TenantConfiguration row for the tenant, including company logo.
		/// </summary>
		Task<TenantConfigurationRow?> GetTenantConfigurationRowByTenantIdAsync(int tenantId);
	}

}
