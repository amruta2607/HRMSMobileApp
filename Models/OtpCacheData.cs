namespace MobileWebApi.Models
{
    /// <summary>
    /// OTP cache data structure stored in memory cache
    /// </summary>
    public class OtpCacheData
    {
        public string OtpHash { get; set; } = string.Empty;
        public DateTime ExpiryTime { get; set; }
        public DateTime ResendAvailableAt { get; set; }
        public int OtpAttemptCount { get; set; }
        public DateTime FirstOtpSentAt { get; set; }
        public int OtpSentCount { get; set; }
    }
}
