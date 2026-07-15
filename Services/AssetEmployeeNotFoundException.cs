namespace MobileWebApi.Services
{
    /// <summary>
    /// Thrown when an employee cannot be found for the current tenant.
    /// </summary>
    public class AssetEmployeeNotFoundException : Exception
    {
        public AssetEmployeeNotFoundException(string message) : base(message)
        {
        }
    }
}
