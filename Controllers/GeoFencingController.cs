using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using System.Security.Claims;

namespace MobileWebApi.Controllers
{
	[ApiController]
	[Authorize]
	[Route("api/geofencing")]
	public class GeoFencingController : ControllerBase
	{
		private readonly IGeoTenantLocationRepository _geoRepo;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GeoFencingController(
			IGeoTenantLocationRepository geoRepo,
			IHttpContextAccessor httpContextAccessor)
		{
			_geoRepo = geoRepo;
			_httpContextAccessor = httpContextAccessor;
		}

		[HttpGet("by-tenant")]
		public async Task<IActionResult> GetTenantGeofence()
		{
			var user = _httpContextAccessor.HttpContext?.User;

			if (user == null || !user.Identity.IsAuthenticated)
			{
				return Unauthorized();
			}

			// ✅ Get UserId from JWT
			var userId = user.FindFirst("UserId")?.Value;

			// ✅ Get Tenant / OrganisationId from JWT
			var tenantId = user.FindFirst("OrganisationId")?.Value;

			if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
			{
				return BadRequest("Invalid token claims");
			}

			int organisationId = int.Parse(tenantId);

			var geoFence = await _geoRepo
				.GetActiveByTenantIdAsync(organisationId);

			if (geoFence == null)
			{
				return Ok(new
				{
					IsEnabled = false
				});
			}

			return Ok(new
			{
				IsEnabled = true,
				BranchId = geoFence.BranchId,
				BranchName = geoFence.BranchName,

				Latitude = geoFence.Latitude.ToString("F6"),
				Longitude = geoFence.Longitude.ToString("F6"),


				Radius = geoFence.Radius,
				OrganisationId = geoFence.OrganisationId
			});

		}
	}
}
