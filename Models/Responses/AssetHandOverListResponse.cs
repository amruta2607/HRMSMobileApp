namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Tenant-specific asset hand over list response for the mobile application.
    /// </summary>
    public class AssetHandOverListResponse
    {
        /// <summary>
        /// Asset hand over records for the authenticated user's organisation.
        /// </summary>
        public List<AssetHandOverDto> Items { get; set; } = new();
    }
}
