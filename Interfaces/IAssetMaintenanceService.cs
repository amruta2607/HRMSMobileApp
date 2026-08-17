using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;

namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Business logic for the asset maintenance module.
    /// </summary>
    public interface IAssetMaintenanceService
    {
        /// <summary>
        /// Validates and creates a new asset maintenance record for the current tenant.
        /// </summary>
        Task<CreateAssetMaintenanceResponse> CreateAssetMaintenanceAsync(AssetMaintenanceRequest request);

        /// <summary>
        /// Validates and updates an existing asset maintenance record for the current tenant.
        /// </summary>
        Task<UpdateAssetMaintenanceResponse> UpdateAssetMaintenanceAsync(int id, UpdateAssetMaintenanceRequest request);

        /// <summary>
        /// Returns all maintenance records for the specified asset, scoped to the current tenant.
        /// </summary>
        Task<AssetMaintenanceHistoryResponse> GetAssetMaintenanceByAssetIdAsync(int assetId);

        /// <summary>
        /// Returns a paged, searchable and sortable list of asset maintenance records for the current tenant.
        /// </summary>
        Task<AssetMaintenanceListResponse> GetAllAssetMaintenanceAsync(AssetMaintenanceQueryParameters query);

        /// <summary>
        /// Deletes an asset maintenance record for the current tenant.
        /// </summary>
        Task<AssetOperationResponse> DeleteAssetMaintenanceAsync(int id, string? ipAddress);

        /// <summary>
        /// Returns lookup data required by the Asset Maintenance module for the current tenant.
        /// </summary>
        Task<AssetMaintenanceLookupResponse> GetAssetMaintenanceLookupsAsync();

        /// <summary>
        /// Returns AssetHistory rows for the specified asset where SourceTable = 'AssetMaintenance'
        /// for the current tenant.
        /// </summary>
        Task<AssetTimelineListResponse> GetAssetMaintenanceTimelineAsync(int assetId);
    }
}
