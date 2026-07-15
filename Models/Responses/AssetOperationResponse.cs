namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Standard success response for asset update and handover operations.
    /// </summary>
    public class AssetOperationResponse
    {
        public bool Success { get; set; } = true;

        public string Message { get; set; } = string.Empty;
    }
}
