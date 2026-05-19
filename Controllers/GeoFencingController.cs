using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Services;

namespace MobileWebApi.Controllers
{
	[ApiController]
	[Authorize]
	[Route("api/geofencing")]
	public class GeoFencingController : TenantBaseController
	{
		private readonly IGeoTenantLocationRepository _geoRepo;

		public GeoFencingController(
			IGeoTenantLocationRepository geoRepo,
			ITenantContext tenantContext,
			ILogger<GeoFencingController> logger)
			: base(tenantContext, logger)
		{
			_geoRepo = geoRepo;
		}

		[HttpGet("by-tenant")]
		public async Task<IActionResult> GetTenantGeofence()
		{
			try
			{
				// Enforce tenant isolation using authenticated user's organisation
				var organisationId = CurrentOrganisationId;

				var geoFence = await _geoRepo.GetActiveByTenantIdAsync(organisationId);

				if (geoFence == null)
				{
					return Ok(new
					{
						IsGeoFencingEnabled = false
					});
				}

				return Ok(new
				{
					IsGeoFencingEnabled = true,
					BranchId = geoFence.BranchId,
					BranchName = geoFence.BranchName,
					Latitude = geoFence.Latitude.ToString("F6"),
					Longitude = geoFence.Longitude.ToString("F6"),
					Radius = geoFence.Radius,
					OrganisationId = geoFence.OrganisationId,
					IsActive=geoFence.IsActive,


				});
			}
			catch (TenantAccessException)
			{
				return TenantAccessDenied();
			}
			catch (Exception ex)
			{
				Logger.LogException(
					ExceptionCodes.GeoFencing.GetTenantGeofence,
					nameof(GetTenantGeofence),
					ex,
					CurrentUserId);

				return StatusCode(
					StatusCodes.Status500InternalServerError,
					new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
			}
		}
	}
}

