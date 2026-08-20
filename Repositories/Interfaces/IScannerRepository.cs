using MobileWebApi.Models.Responses;

namespace MobileWebApi.Repositories.Interfaces
{
    /// <summary>
    /// Repository for scanner-based asset lookup.
    /// </summary>
    public interface IScannerRepository
    {
        /// <summary>
        /// Retrieves an asset by id for the authenticated user's organisation.
        /// </summary>
        /// <param name="assetId">Asset identifier from the scanned QR code.</param>
        Task<AssetScannerResponse?> GetAssetAsync(int assetId);
    }
}
