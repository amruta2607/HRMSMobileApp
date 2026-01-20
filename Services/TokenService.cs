using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MobileWebApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        // Blacklist for JWT access tokens (key: token jti claim or full token, value: expiry time)
        private static readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens = new();
        
        // Token expiry settings (default: 24 hours, can be overridden in appsettings.json)
        private readonly int _tokenExpiryHours = 24;

        public TokenService(IConfiguration config)
        {
            _config = config;
            
            // Read from config if available
            if (int.TryParse(_config["Jwt:TokenExpiryHours"], out int tokenHours))
                _tokenExpiryHours = tokenHours;
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.WorkRoleName ?? "User"),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("OrganisationId", user.OrganisationId.ToString()),
                new Claim("IsHrUser", user.IsHrUser.ToString()),
                new Claim("IsTenantAdmin", user.IsTenantAdmin.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(_tokenExpiryHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateTokenForEmployee(int employeeId, int tenantId, string name, int? userId = null)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, name),
                new Claim("EmployeeId", employeeId.ToString()),
                new Claim("OrganisationId", tenantId.ToString()),
                new Claim(ClaimTypes.Role, "Employee")
            };

            // Add UserId claim if available (from SystemUserId in Employee table)
            if (userId.HasValue)
            {
                claims.Add(new Claim("UserId", userId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(_tokenExpiryHours),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)),
                ValidateLifetime = false, // We want to get claims from expired tokens
                ValidIssuer = _config["Jwt:Issuer"],
                ValidAudience = _config["Jwt:Audience"]
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

        /// <summary>
        /// Blacklist a JWT access token so it cannot be used anymore
        /// </summary>
        public void BlacklistToken(string token, DateTime expiry)
        {
            // Use token's jti (JWT ID) if available, otherwise use a hash of the token
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                if (tokenHandler.CanReadToken(token))
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                    
                    if (!string.IsNullOrEmpty(jti))
                    {
                        _blacklistedTokens[jti] = expiry;
                        return;
                    }
                }
            }
            catch
            {
                // If we can't parse the token, fall back to hashing
            }

            // Fallback: use a hash of the token as the key
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            var tokenKey = Convert.ToBase64String(hash);
            _blacklistedTokens[tokenKey] = expiry;

            // Cleanup expired blacklist entries
            CleanupBlacklistedTokens();
        }

        /// <summary>
        /// Check if a JWT access token is blacklisted
        /// </summary>
        public bool IsTokenBlacklisted(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                if (tokenHandler.CanReadToken(token))
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
                    
                    if (!string.IsNullOrEmpty(jti) && _blacklistedTokens.TryGetValue(jti, out var expiry))
                    {
                        // If token expiry is passed, consider it not blacklisted (would be expired anyway)
                        return DateTime.UtcNow < expiry;
                    }
                }
            }
            catch
            {
                // If we can't parse the token, check using hash
            }

            // Fallback: check using hash
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
            var tokenKey = Convert.ToBase64String(hash);
            
            if (_blacklistedTokens.TryGetValue(tokenKey, out var hashExpiry))
            {
                return DateTime.UtcNow < hashExpiry;
            }

            return false;
        }

        public DateTime GetTokenExpiry()
        {
            return DateTime.Now.AddHours(_tokenExpiryHours);
        }

        /// <summary>
        /// Remove expired entries from the blacklist
        /// </summary>
        private void CleanupBlacklistedTokens()
        {
            var expiredEntries = _blacklistedTokens.Where(x => DateTime.UtcNow >= x.Value).Select(x => x.Key).ToList();
            foreach (var entry in expiredEntries)
            {
                _blacklistedTokens.TryRemove(entry, out _);
            }
        }
    }
}

