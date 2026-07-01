using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Services;

namespace MobileWebApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/locationtracking")]
    public class LocationTrackingController : TenantBaseController
    {
        private readonly ILocationTrackingService _locationTrackingService;

        public LocationTrackingController(
            ILocationTrackingService locationTrackingService,
            ITenantContext tenantContext,
            ILogger<LocationTrackingController> logger)
            : base(tenantContext, logger)
        {
            _locationTrackingService = locationTrackingService;
        }

        /// <summary>
        /// Receives GPS coordinates from the mobile app and stores them in LocationTracking.
        /// POST: api/locationtracking
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> RecordLocation([FromBody] LocationTrackingRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new LocationTrackingResponse
                    {
                        Success = false,
                        Message = GeneralMessages.RequestBodyCannotBeNull
                    });
                }

                var currentUserId = CurrentUserId;
                if (!currentUserId.HasValue || currentUserId.Value <= 0)
                {
                    return Unauthorized(new LocationTrackingResponse
                    {
                        Success = false,
                        Message = AuthMessages.InvalidAuthenticationToken
                    });
                }

                if (request.userId <= 0)
                {
                    return BadRequest(new LocationTrackingResponse
                    {
                        Success = false,
                        Message = LocationTrackingMessages.UserIdRequired
                    });
                }

                if (request.userId != currentUserId.Value)
                {
                    Logger.LogWarning(
                        LogMessages.TenantAccess.UserAccessViolation,
                        currentUserId.Value,
                        request.userId);
                    return UserAccessDenied();
                }

                var result = await _locationTrackingService.RecordLocationAsync(
                    request,
                    currentUserId.Value,
                    CurrentOrganisationId);

                if (!result.Success)
                {
                    if (result.Message == LocationTrackingMessages.EmployeeNotFound
                        || result.Message == LocationTrackingMessages.TenantNotFound)
                    {
                        return NotFound(result);
                    }

                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.LocationTracking.RecordLocation,
                    nameof(RecordLocation),
                    ex,
                    CurrentUserId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new LocationTrackingResponse
                    {
                        Success = false,
                        Message = GeneralMessages.SomethingWentWrongContactAdmin
                    });
            }
        }
    }
}
