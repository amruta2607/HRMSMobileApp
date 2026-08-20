namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Response wrapper for asset maintenance history retrieved by asset id for the current tenant.
    /// </summary>
    public class AssetMaintenanceHistoryResponse
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
        /// Maintenance records for the requested asset, ordered by Date then InsertDate descending.
        /// Empty when the asset has no maintenance history.
        /// </summary>
        public List<AssetMaintenanceDto> Data { get; set; } = new();
    }
}
