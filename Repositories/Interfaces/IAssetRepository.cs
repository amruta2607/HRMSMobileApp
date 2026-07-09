using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;

namespace MobileWebApi.Repositories.Interfaces
{
    /// <summary>
    /// Repository for tenant-specific asset list data access.
    /// </summary>
    public interface IAssetRepository
    {
        /// <summary>
        /// Retrieves all assets for the authenticated user's organisation.
        /// </summary>
        Task<AssetListResponse> GetAssetsAsync();

        /// <summary>
        /// Creates a new asset for the authenticated user's organisation.
        /// </summary>
        Task<CreateAssetResponse> CreateAssetAsync(CreateAssetRequest request);
    }
}
