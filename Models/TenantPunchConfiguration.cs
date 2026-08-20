namespace MobileWebApi.Models
{
    /// <summary>
    /// Tenant configuration flags that control multiple punch behavior.
    /// </summary>
    public class TenantPunchConfiguration
    {
        public bool IsLocationTrackingEnabled { get; set; }
        public bool IsMultiplePunchOutEnabled { get; set; }
    }
}
