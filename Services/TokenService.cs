using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using System.Collections.Concurrent;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MobileWebApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Blacklist for JWT access tokens (key: token jti claim or full token hash, value: expiry time)
        private static readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();

        // In-memory refresh token store (key: SHA-256 hash of token, value: metadata)
        private static readonly ConcurrentDictionary<string, RefreshTokenEntry> _refreshTokens = new();
        private static readonly object _refreshTokenLock = new();

        private readonly int _accessTokenExpiryInSeconds;
        private readonly int _refreshTokenExpiryInHours;
        private const string LocalExpiryFormat = "yyyy-MM-ddTHH:mm:ss";

        public TokenService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;

            if (!int.TryParse(_config["Jwt:AccessTokenExpiryInSeconds"], out _accessTokenExpiryInSeconds) || _accessTokenExpiryInSeconds <= 0)
            {
                throw new InvalidOperationException(
                    "Jwt:AccessTokenExpiryInSeconds is missing or invalid. Set a positive number of seconds in appsettings.json.");
            }

            if (!int.TryParse(_config["Jwt:RefreshTokenExpiryInHours"], out _refreshTokenExpiryInHours) || _refreshTokenExpiryInHours <= 0)
            {
                throw new InvalidOperationException(
                    "Jwt:RefreshTokenExpiryInHours is missing or invalid. Set a positive number of hours in appsettings.json.");
            }
        }

        public string GenerateToken(User user)
        {
            var (accessToken, _, _) = CreateAccessToken(user);
            return accessToken;
        }

        public string GenerateTokenForEmployee(int employeeId, int tenantId, string name, int? userId = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jti = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim("EmployeeId", employeeId.ToString()),
                new Claim("OrganisationId", tenantId.ToString()),
                new Claim(ClaimTypes.Role, "Employee"),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            if (userId.HasValue)
                claims.Add(new Claim("UserId", userId.Value.ToString()));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddSeconds(_accessTokenExpiryInSeconds),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<AuthResponse> GenerateTokensAsync(User user)
        {
            var issuedAt = DateTime.Now;
            var accessTokenExpiry = issuedAt.AddSeconds(_accessTokenExpiryInSeconds);
            var refreshTokenExpiry = issuedAt.AddHours(_refreshTokenExpiryInHours);

            var (accessToken, jti, expiresAt) = CreateAccessToken(user, accessTokenExpiry);
            var refreshToken = GenerateSecureRefreshToken();
            var refreshTokenHash = HashToken(refreshToken);

            _refreshTokens[refreshTokenHash] = new RefreshTokenEntry
            {
                UserId = user.UserId,
                Username = user.Username,
                WorkRoleName = user.WorkRoleName ?? "User",
                OrganisationId = user.OrganisationId,
                BranchId = user.BranchId,
                IsHrUser = user.IsHrUser,
                IsTenantAdmin = user.IsTenantAdmin,
                JwtId = jti,
                ExpiryDate = refreshTokenExpiry,
                CreatedDate = issuedAt,
                IsUsed = false,
                IsRevoked = false,
                CreatedByIp = GetClientIpAddress()
            };

            CleanupExpiredRefreshTokens();

            return Task.FromResult(new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = (int)(expiresAt - DateTime.Now).TotalSeconds,
                AccessTokenExpiresIn = _accessTokenExpiryInSeconds,
                RefreshTokenExpiresIn = _refreshTokenExpiryInHours,
                AccessTokenExpiry = FormatLocalExpiry(accessTokenExpiry),
                RefreshTokenExpiry = FormatLocalExpiry(refreshTokenExpiry)
            });
        }

        public Task<AccessTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new TokenRefreshException(AuthMessages.RefreshTokenRequired);

            var refreshTokenHash = HashToken(request.RefreshToken);

            lock (_refreshTokenLock)
            {
                if (!_refreshTokens.TryGetValue(refreshTokenHash, out var storedToken))
                    throw new TokenRefreshException(AuthMessages.InvalidRefreshToken);

                if (storedToken.IsUsed)
                {
                    RevokeAllUserRefreshTokens(storedToken.UserId);
                    throw new TokenRefreshException(AuthMessages.RefreshTokenAlreadyUsed);
                }

                if (storedToken.IsRevoked)
                    throw new TokenRefreshException(AuthMessages.RefreshTokenRevokedLoginRequired);

                if (storedToken.ExpiryDate <= DateTime.Now)
                    throw new TokenRefreshException(AuthMessages.RefreshTokenExpired);

                var user = new User
                {
                    UserId = storedToken.UserId,
                    Username = storedToken.Username,
                    WorkRoleName = storedToken.WorkRoleName,
                    OrganisationId = storedToken.OrganisationId,
                    BranchId = storedToken.BranchId,
                    IsHrUser = storedToken.IsHrUser,
                    IsTenantAdmin = storedToken.IsTenantAdmin
                };

                var (newAccessToken, newJti, _) = CreateAccessToken(user);
                storedToken.JwtId = newJti;

                CleanupExpiredRefreshTokens();

                return Task.FromResult(new AccessTokenResponse
                {
                    AccessToken = newAccessToken
                });
            }
        }

        public Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Task.FromResult(false);

            var refreshTokenHash = HashToken(refreshToken);

            lock (_refreshTokenLock)
            {
                if (!_refreshTokens.TryGetValue(refreshTokenHash, out var storedToken) || storedToken.IsRevoked)
                    return Task.FromResult(false);

                storedToken.IsRevoked = true;
                storedToken.RevokedDate = DateTime.UtcNow;
                return Task.FromResult(true);
            }
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
                ValidateLifetime = false,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public void BlacklistToken(string token, DateTime expiry)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                if (tokenHandler.CanReadToken(token))
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
                              ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

                    if (!string.IsNullOrEmpty(jti))
                    {
                        _blacklistedTokens[jti] = expiry;
                        return;
                    }
                }
            }
            catch
            {
                // Fall back to hashing the full token
            }

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            var tokenKey = Convert.ToBase64String(hash);
            _blacklistedTokens[tokenKey] = expiry;

            CleanupBlacklistedTokens();
        }

        public bool IsTokenBlacklisted(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                if (tokenHandler.CanReadToken(token))
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value
                              ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

                    if (!string.IsNullOrEmpty(jti) && _blacklistedTokens.TryGetValue(jti, out var expiry))
                        return DateTime.UtcNow < expiry;
                }
            }
            catch
            {
                // Fall back to hash lookup
            }

            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            var tokenKey = Convert.ToBase64String(hash);

            if (_blacklistedTokens.TryGetValue(tokenKey, out var hashExpiry))
                return DateTime.UtcNow < hashExpiry;

            return false;
        }

        public DateTime GetTokenExpiry()
        {
            return DateTime.Now.AddSeconds(_accessTokenExpiryInSeconds);
        }

        private (string AccessToken, string Jti, DateTime ExpiresAt) CreateAccessToken(User user, DateTime? expiresAt = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jti = Guid.NewGuid().ToString();
            var tokenExpiry = expiresAt ?? DateTime.Now.AddSeconds(_accessTokenExpiryInSeconds);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.WorkRoleName ?? "User"),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("OrganisationId", user.OrganisationId.ToString()),
                new Claim("BranchId", user.BranchId.ToString()),
                new Claim("IsHrUser", user.IsHrUser.ToString()),
                new Claim("IsTenantAdmin", user.IsTenantAdmin.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, jti)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: tokenExpiry,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), jti, tokenExpiry);
        }

        private static string FormatLocalExpiry(DateTime value)
        {
            return value.ToString(LocalExpiryFormat, CultureInfo.InvariantCulture);
        }

        private static string GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[64];
            RandomNumberGenerator.Fill(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private static string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }

        private string? GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
        }

        private static void RevokeAllUserRefreshTokens(int userId)
        {
            foreach (var entry in _refreshTokens.Values.Where(t => t.UserId == userId && !t.IsRevoked && !t.IsUsed))
            {
                entry.IsRevoked = true;
                entry.RevokedDate = DateTime.UtcNow;
            }
        }

        private static void CleanupExpiredRefreshTokens()
        {
            var expiredKeys = _refreshTokens
                .Where(x => x.Value.ExpiryDate <= DateTime.Now)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in expiredKeys)
                _refreshTokens.TryRemove(key, out _);
        }

        private void CleanupBlacklistedTokens()
        {
            var expiredEntries = _blacklistedTokens
                .Where(x => DateTime.UtcNow >= x.Value)
                .Select(x => x.Key)
                .ToList();

            foreach (var entry in expiredEntries)
                _blacklistedTokens.TryRemove(entry, out _);
        }

        /// <summary>In-memory refresh token metadata (no database persistence).</summary>
        private sealed class RefreshTokenEntry
        {
            public int UserId { get; set; }
            public string Username { get; set; } = string.Empty;
            public string WorkRoleName { get; set; } = "User";
            public int OrganisationId { get; set; }
            public int BranchId { get; set; }
            public bool IsHrUser { get; set; }
            public bool IsTenantAdmin { get; set; }
            public string JwtId { get; set; } = string.Empty;
            public DateTime ExpiryDate { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? RevokedDate { get; set; }
            public bool IsUsed { get; set; }
            public bool IsRevoked { get; set; }
            public string? CreatedByIp { get; set; }
            public string? ReplacedByToken { get; set; }
        }
    }
}
