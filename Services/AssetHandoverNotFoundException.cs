namespace MobileWebApi.Services
{
    /// <summary>
    /// Thrown when an asset handover record cannot be found for the current tenant.
    /// </summary>
    public class AssetHandoverNotFoundException : Exception
    {
        public AssetHandoverNotFoundException(string message) : base(message)
        {
        }
    }
}
