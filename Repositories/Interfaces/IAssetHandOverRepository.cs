using MobileWebApi.Models.Requests;
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

        /// <summary>
        /// Retrieves lookup data for the Asset HandOver screen.
        /// </summary>
        Task<AssetHandOverLookupsResponse> GetLookupsAsync();

        /// <summary>
        /// Transfers an asset to another employee and records handover history.
        /// </summary>
        Task<AssetOperationResponse> AssetHandoverAsync(AssetHandoverRequest request);

        /// <summary>
        /// Updates an existing asset handover record for the authenticated user's organisation.
        /// </summary>
        Task<AssetOperationResponse> UpdateAssetHandoverAsync(int handoverId, UpdateAssetHandoverRequest request);

        /// <summary>
        /// Deletes an asset handover record for the authenticated user's organisation.
        /// </summary>
        Task<AssetOperationResponse> DeleteAssetHandoverAsync(int handoverId, string? ipAddress);

        /// <summary>
        /// Returns AssetHistory rows for the specified asset where SourceTable = 'AssetHandOver',
        /// scoped to the current tenant, ordered by ActionDate descending.
        /// </summary>
        Task<AssetTimelineListResponse> GetAssetHandOverTimelineAsync(int assetId);
    }
}
