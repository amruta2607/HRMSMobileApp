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
	}
}
