using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IGeoTenantLocationRepository
    {
		Task<GeoTenantLocationRow> GetActiveByTenantIdAsync(int tenantId);
	}
}
