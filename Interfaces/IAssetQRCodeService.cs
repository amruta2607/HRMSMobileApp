using System.Data;

namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Generates asset codes and QR images for assets.
    /// </summary>
    public interface IAssetQRCodeService
    {
        /// <summary>
        /// Generates the next tenant-scoped asset code.
        /// </summary>
        string GenerateAssetCode(IDbConnection connection, int tenantId, IDbTransaction? transaction = null);

        /// <summary>
        /// Ensures the asset has a generated QR code and updates the database row.
        /// </summary>
        AssetQRResult EnsureQRCode(
            IDbConnection connection,
            IDbTransaction transaction,
            int assetId,
            int tenantId);
    }

    /// <summary>
    /// Result of QR code generation for an asset.
    /// </summary>
    public class AssetQRResult
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = string.Empty;
        public string QRCodePath { get; set; } = string.Empty;
        public string QRCodeText { get; set; } = string.Empty;
        public bool QRCodeGenerated { get; set; }
        public DateTime? QRCodeGeneratedDate { get; set; }
    }
}
