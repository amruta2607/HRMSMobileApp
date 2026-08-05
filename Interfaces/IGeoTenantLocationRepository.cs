using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IGeoTenantLocationRepository
    {
        /// <summary>
        /// Returns the active geofence for the given organisation and branch, or null if none exists.
        /// </summary>
        Task<GeoTenantLocationRow?> GetActiveByTenantAndBranchAsync(int organisationId, int branchId);
    }
}
