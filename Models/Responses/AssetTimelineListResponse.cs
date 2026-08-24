namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Response wrapper for asset maintenance timeline rows for the current tenant.
    /// </summary>
    public class AssetTimelineListResponse
    {
        /// <summary>
        /// Indicates whether the operation succeeded.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Result message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timeline entries for the requested asset, ordered by ActionDate descending.
        /// </summary>
        public List<AssetTimelineResponse> Data { get; set; } = new();
    }
}
