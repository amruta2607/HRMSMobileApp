namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Response returned after successfully updating an asset maintenance record.
    /// </summary>
    public class UpdateAssetMaintenanceResponse
    {
        /// <summary>
        /// Indicates whether the operation succeeded.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Identifier of the updated asset maintenance record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Success message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
