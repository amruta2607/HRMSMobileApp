namespace MobileWebApi.Services
{
    /// <summary>
    /// Thrown when an asset cannot be found for the current tenant.
    /// </summary>
    public class AssetNotFoundException : Exception
    {
        public AssetNotFoundException(string message) : base(message)
        {
        }
    }
}
