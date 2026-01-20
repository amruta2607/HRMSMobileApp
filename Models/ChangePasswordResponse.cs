namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for change password/PIN operation
    /// </summary>
    public class ChangePasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

