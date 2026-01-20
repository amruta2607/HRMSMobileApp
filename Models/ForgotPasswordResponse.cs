namespace MobileWebApi.Models
{
    /// <summary>
    /// Response model for forgot password/PIN request
    /// </summary>
    public class ForgotPasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Masked contact info where OTP was sent (e.g., "****1234" for mobile)
        /// </summary>
        public string? SentTo { get; set; }

        /// <summary>
        /// OTP for testing purposes only - REMOVE IN PRODUCTION
        /// </summary>
       
    }
}



