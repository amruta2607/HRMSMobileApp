using MobileWebApi.Interfaces;
using MobileWebApi.Constants;
using MobileWebApi.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace MobileWebApi.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<OtpService> _logger;
        private readonly TimeSpan _otpExpiry = TimeSpan.FromMinutes(10); // OTP valid for 10 minutes (for email/username)
        private readonly TimeSpan _mobileOtpExpiry = TimeSpan.FromMinutes(5); // Mobile OTP valid for 5 minutes
        private readonly TimeSpan _resendCooldown = TimeSpan.FromSeconds(30); // 30 seconds cooldown
        private readonly TimeSpan _rateLimitWindow = TimeSpan.FromHours(1); // Rate limit window: 1 hour
        private readonly int _maxOtpsPerHour = 5; // Maximum 5 OTPs per hour

        public OtpService(IMemoryCache cache, ILogger<OtpService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public string GenerateOtp(string identifier)
        {
            // Generate a 6-digit OTP
            string otp = GenerateRandomOtp(6);

            // Store OTP in cache with expiration
            var cacheKey = GetCacheKey(identifier);
            _cache.Set(cacheKey, otp, _otpExpiry);

            _logger.LogInformation(LogMessages.Otp.OtpGenerated, MaskIdentifier(identifier));

            return otp;
        }

        public (string otp, int resendAfterSeconds, bool canSend) GenerateMobileOtp(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
            {
                return (string.Empty, 0, false);
            }

            var cacheKey = GetMobileOtpCacheKey(mobileNumber);
            var now = DateTime.UtcNow;

            // Check if OTP already exists and resend cooldown hasn't elapsed
            if (_cache.TryGetValue(cacheKey, out OtpCacheData? existingData))
            {
                // Check rate limiting (max 5 OTPs per hour)
                if (existingData.FirstOtpSentAt.Add(_rateLimitWindow) > now && existingData.OtpSentCount >= _maxOtpsPerHour)
                {
                    var resetTime = existingData.FirstOtpSentAt.Add(_rateLimitWindow);
                    var remainingSeconds = (int)(resetTime - now).TotalSeconds;
                    _logger.LogWarning(LogMessages.Otp.RateLimitExceeded, 
                        MaskIdentifier(mobileNumber), remainingSeconds);
                    return (string.Empty, remainingSeconds, false);
                }

                // Check resend cooldown
                if (existingData.ResendAvailableAt > now)
                {
                    var remainingSeconds = (int)(existingData.ResendAvailableAt - now).TotalSeconds;
                    _logger.LogWarning(LogMessages.Otp.ResendCooldownActive, 
                        MaskIdentifier(mobileNumber), remainingSeconds);
                    return (string.Empty, remainingSeconds, false);
                }

                // Reset rate limit window if hour has passed
                if (existingData.FirstOtpSentAt.Add(_rateLimitWindow) <= now)
                {
                    existingData.FirstOtpSentAt = now;
                    existingData.OtpSentCount = 0;
                }
            }

            // Generate 6-digit numeric OTP
            string otp = GenerateRandomOtp(6);

            // Hash OTP using SHA256
            string otpHash = HashOtp(otp);

            // Create or update cache data
            var cacheData = existingData ?? new OtpCacheData
            {
                FirstOtpSentAt = now,
                OtpSentCount = 0
            };

            cacheData.OtpHash = otpHash;
            cacheData.ExpiryTime = now.Add(_mobileOtpExpiry);
            cacheData.ResendAvailableAt = now.Add(_resendCooldown);
            cacheData.OtpAttemptCount = 0;
            cacheData.OtpSentCount++;

            // Store in cache with expiry time
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = cacheData.ExpiryTime
            };

            _cache.Set(cacheKey, cacheData, cacheOptions);

            _logger.LogInformation(LogMessages.Otp.MobileOtpGenerated, 
                MaskIdentifier(mobileNumber), (int)_resendCooldown.TotalSeconds);

            return (otp, (int)_resendCooldown.TotalSeconds, true);
        }

        public bool ValidateOtp(string identifier, string otp)
        {
            var cacheKey = GetCacheKey(identifier);

            if (_cache.TryGetValue(cacheKey, out string? storedOtp))
            {
                return string.Equals(storedOtp, otp, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public bool ValidateMobileOtp(string mobileNumber, string otp)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber) || string.IsNullOrWhiteSpace(otp))
            {
                return false;
            }

            var cacheKey = GetMobileOtpCacheKey(mobileNumber);

            if (!_cache.TryGetValue(cacheKey, out OtpCacheData? cacheData))
            {
                _logger.LogWarning(LogMessages.Otp.MobileOtpNotFoundOrExpired, MaskIdentifier(mobileNumber));
                return false;
            }

            // Check if OTP has expired
            if (DateTime.UtcNow > cacheData.ExpiryTime)
            {
                _cache.Remove(cacheKey);
                _logger.LogWarning(LogMessages.Otp.MobileOtpExpired, MaskIdentifier(mobileNumber));
                return false;
            }

            // Increment attempt count
            cacheData.OtpAttemptCount++;

            // Hash the incoming OTP and compare
            string otpHash = HashOtp(otp);
            bool isValid = cacheData.OtpHash.Equals(otpHash, StringComparison.Ordinal);

            if (isValid)
            {
                // Remove OTP from cache on successful validation
                _cache.Remove(cacheKey);
                _logger.LogInformation(LogMessages.Otp.MobileOtpValidatedSuccessfully, MaskIdentifier(mobileNumber));
            }
            else
            {
                _logger.LogWarning(LogMessages.Otp.InvalidMobileOtpAttempt, 
                    cacheData.OtpAttemptCount, MaskIdentifier(mobileNumber));
            }

            return isValid;
        }

        public void RemoveOtp(string identifier)
        {
            var cacheKey = GetCacheKey(identifier);
            _cache.Remove(cacheKey);
        }

        public void RemoveMobileOtp(string mobileNumber)
        {
            var cacheKey = GetMobileOtpCacheKey(mobileNumber);
            _cache.Remove(cacheKey);
        }

        public int GetResendCooldownSeconds(string mobileNumber)
        {
            var cacheKey = GetMobileOtpCacheKey(mobileNumber);
            
            if (_cache.TryGetValue(cacheKey, out OtpCacheData? cacheData))
            {
                var now = DateTime.UtcNow;
                if (cacheData.ResendAvailableAt > now)
                {
                    return (int)(cacheData.ResendAvailableAt - now).TotalSeconds;
                }
            }

            return 0;
        }

        private static string GenerateRandomOtp(int length)
        {
            const string digits = "0123456789";
            char[] otp = new char[length];

            using var rng = RandomNumberGenerator.Create();
            byte[] randomBytes = new byte[length];
            rng.GetBytes(randomBytes);

            for (int i = 0; i < length; i++)
            {
                otp[i] = digits[randomBytes[i] % digits.Length];
            }

            return new string(otp);
        }

        private static string HashOtp(string otp)
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(otp));
            return Convert.ToBase64String(hashBytes);
        }

        private static string GetCacheKey(string identifier)
        {
            return $"otp:{identifier.ToLowerInvariant()}";
        }

        private static string GetMobileOtpCacheKey(string mobileNumber)
        {
            // Normalize mobile number (remove any spaces, dashes, etc.)
            var normalized = new string(mobileNumber.Where(char.IsDigit).ToArray());
            return $"otp:{normalized}";
        }

        private static string MaskIdentifier(string identifier)
        {
            if (string.IsNullOrEmpty(identifier) || identifier.Length <= 4)
                return "****";

            return new string('*', identifier.Length - 4) + identifier[^4..];
        }
    }
}



