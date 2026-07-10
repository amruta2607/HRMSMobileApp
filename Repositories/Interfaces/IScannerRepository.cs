using MobileWebApi.Models.Responses;

namespace MobileWebApi.Repositories.Interfaces
{
    /// <summary>
    /// Repository for scanner-based asset lookup.
    /// </summary>
    public interface IScannerRepository
    {
        /// <summary>
        /// Retrieves an asset by scanner code for the authenticated user's organisation.
        /// </summary>
        /// <param name="code">Asset code, QR code text, or asset number.</param>
        Task<AssetScannerResponse?> GetAssetAsync(string code);
    }
}
