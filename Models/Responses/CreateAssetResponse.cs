namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Response returned after successfully creating an asset.
    /// </summary>
    public class CreateAssetResponse
    {
        /// <summary>
        /// Newly created asset identifier.
        /// </summary>
        public int AssetId { get; set; }

        /// <summary>
        /// Auto-generated asset number.
        /// </summary>
        public string Number { get; set; } = string.Empty;

        /// <summary>
        /// Auto-generated asset code.
        /// </summary>
        public string AssetCode { get; set; } = string.Empty;

        /// <summary>
        /// Success message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
