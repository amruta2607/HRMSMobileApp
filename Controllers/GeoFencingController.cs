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
		private readonly ITenantConfigurationRepository _tenantConfigurationRepository;

		public GeoFencingController(
			IGeoTenantLocationRepository geoRepo,
			ITenantConfigurationRepository tenantConfigurationRepository,
			ITenantContext tenantContext,
			ILogger<GeoFencingController> logger)
			: base(tenantContext, logger)
		{
			_geoRepo = geoRepo;
			_tenantConfigurationRepository = tenantConfigurationRepository;
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

				var tenantConfig = await _tenantConfigurationRepository
					.GetByTenantIdAsync(organisationId, geoFence.BranchId);
				var isGeoFencingEnabled = tenantConfig?.IsGeoFencingEnabled ?? false;

				return Ok(new
				{
					IsGeoFencingEnabled = isGeoFencingEnabled,
					BranchId = geoFence.BranchId,
					BranchName = geoFence.BranchName,
					Latitude = isGeoFencingEnabled ? geoFence.Latitude.ToString("F6") : null,
					Longitude = isGeoFencingEnabled ? geoFence.Longitude.ToString("F6") : null,
					Radius = isGeoFencingEnabled ? geoFence.Radius : (int?)null,
					OrganisationId = geoFence.OrganisationId,
					IsActive = geoFence.IsActive
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

