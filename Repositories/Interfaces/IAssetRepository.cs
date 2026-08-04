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
        /// Retrieves all lookup values required by the Create Asset screen.
        /// </summary>
        Task<AssetLookupsResponse> GetLookupsAsync();

        /// <summary>
        /// Creates a new asset for the authenticated user's organisation.
        /// </summary>
        Task<CreateAssetResponse> CreateAssetAsync(CreateAssetRequest request);

        /// <summary>
        /// Updates editable fields on an existing asset for the authenticated user's organisation.
        /// </summary>
        Task<AssetOperationResponse> UpdateAssetAsync(int assetId, UpdateAssetRequest request);

        /// <summary>
        /// Deletes an asset and all related dependent records for the authenticated user's organisation.
        /// </summary>
        Task<AssetOperationResponse> DeleteAssetAsync(int assetId, string? ipAddress);

        /// <summary>
        /// Retrieves the QR code for an asset belonging to the authenticated user's organisation.
        /// </summary>
        Task<AssetQrCodeResponse> GetAssetQrCodeAsync(int assetId);

        /// <summary>
        /// Returns AssetHistory rows for the specified asset where SourceTable = 'Asset',
        /// scoped to the current tenant, ordered by ActionDate descending.
        /// </summary>
        Task<AssetTimelineListResponse> GetAssetTimelineAsync(int assetId);
    }
}
