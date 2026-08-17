namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Tenant-specific asset list response for the mobile application.
    /// </summary>
    public class AssetListResponse
    {
        /// <summary>
        /// Assets belonging to the authenticated user's organisation.
        /// </summary>
        public List<AssetDto> Assets { get; set; } = new();
    }
}
