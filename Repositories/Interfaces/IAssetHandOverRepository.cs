using MobileWebApi.Models.Responses;

namespace MobileWebApi.Repositories.Interfaces
{
    /// <summary>
    /// Repository for tenant-specific asset hand over data access.
    /// </summary>
    public interface IAssetHandOverRepository
    {
        /// <summary>
        /// Retrieves all asset hand over records for the authenticated user's organisation.
        /// </summary>
        Task<AssetHandOverListResponse> GetListAsync();
    }
}
