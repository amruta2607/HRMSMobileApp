using MobileWebApi.Models;
using System.Security.Claims;

namespace MobileWebApi.Interfaces
{
	public interface ITokenService
	{
		string GenerateToken(User user);
		string GenerateTokenForEmployee(int employeeId, int tenantId, string name, int? userId = null);
		ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
		void BlacklistToken(string token, DateTime expiry);
		bool IsTokenBlacklisted(string token);
		DateTime GetTokenExpiry();

		/// <summary>Creates a new access + refresh token pair and persists the refresh token.</summary>
		Task<AuthResponse> GenerateTokensAsync(User user);

		/// <summary>Issues a new access token from a valid refresh token.</summary>
		Task<AccessTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);

		/// <summary>Revokes a refresh token (logout).</summary>
		Task<bool> RevokeRefreshTokenAsync(string refreshToken);
	}
}
