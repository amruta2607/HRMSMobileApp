namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Response returned after successfully creating an asset maintenance record.
    /// </summary>
    public class CreateAssetMaintenanceResponse
    {
        /// <summary>
        /// Indicates whether the operation succeeded.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Newly created asset maintenance identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Success message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
