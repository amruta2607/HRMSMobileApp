namespace MobileWebApi.Services
{
    /// <summary>
    /// Thrown when a QR code cannot be found for the specified asset.
    /// </summary>
    public class AssetQrCodeNotFoundException : Exception
    {
        public AssetQrCodeNotFoundException(string message) : base(message)
        {
        }
    }
}
