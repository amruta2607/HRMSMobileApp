using MobileWebApi.Models;

namespace MobileWebApi.Repositories.Interfaces
{
    /// <summary>
    /// Repository for tenant-specific asset dashboard data access.
    /// </summary>
    public interface IAssetDashboardRepository
    {
        /// <summary>
        /// Retrieves the asset dashboard for the authenticated user's organisation.
        /// </summary>
        Task<AssetDashboardResponse> GetDashboardAsync();
    }
}
