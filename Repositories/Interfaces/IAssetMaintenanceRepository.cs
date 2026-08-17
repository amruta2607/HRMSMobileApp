using MobileWebApi.Models;
using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;

namespace MobileWebApi.Repositories.Interfaces
{
    /// <summary>
    /// Repository for tenant-scoped asset maintenance data access.
    /// </summary>
    public interface IAssetMaintenanceRepository
    {
        /// <summary>
        /// Creates a new asset maintenance record for the authenticated user's organisation.
        /// The already-uploaded <paramref name="attachments"/> are serialized into the Attachment column.
        /// </summary>
        Task<CreateAssetMaintenanceResponse> CreateAsync(AssetMaintenanceRequest request, List<FileAttachment>? attachments);

        /// <summary>
        /// Updates an existing asset maintenance record for the authenticated user's organisation.
        /// When <paramref name="attachments"/> is null the existing Attachment value is kept unchanged;
        /// otherwise it is replaced with the serialized list.
        /// </summary>
        Task<UpdateAssetMaintenanceResponse> UpdateAsync(int id, UpdateAssetMaintenanceRequest request, List<FileAttachment>? attachments);

        /// <summary>
        /// Retrieves all maintenance records for the specified asset, scoped to the current tenant,
        /// ordered by Date then InsertDate descending.
        /// </summary>
        Task<AssetMaintenanceHistoryResponse> GetByAssetIdAsync(int assetId);

        /// <summary>
        /// Retrieves a paged, searchable and sortable list of asset maintenance records
        /// for the authenticated user's organisation.
        /// </summary>
        Task<AssetMaintenanceListResponse> GetAllAsync(AssetMaintenanceQueryParameters query);

        /// <summary>
        /// Deletes an asset maintenance record for the authenticated user's organisation.
        /// </summary>
        Task<AssetOperationResponse> DeleteAsync(int id, string? ipAddress);

        /// <summary>
        /// Returns lookup data (assets and responsible persons) required by the Asset Maintenance module,
        /// scoped to the current tenant.
        /// </summary>
        Task<AssetMaintenanceLookupResponse> GetAssetMaintenanceLookupsAsync();

        /// <summary>
        /// Returns AssetHistory rows for the specified asset where SourceTable = 'AssetMaintenance',
        /// scoped to the current tenant, ordered by ActionDate descending.
        /// </summary>
        Task<AssetTimelineListResponse> GetAssetMaintenanceTimelineAsync(int assetId);
    }
}
