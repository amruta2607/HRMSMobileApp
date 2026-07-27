namespace MobileWebApi.Services
{
    /// <summary>
    /// Thrown when an asset exists but has no QR code available.
    /// </summary>
    public class AssetQrCodeNotFoundException : Exception
    {
        public AssetQrCodeNotFoundException(string message) : base(message)
        {
        }
    }
}
