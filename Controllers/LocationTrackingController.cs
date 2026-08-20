using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;
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
        /// Returns today's complete location tracking path for the requested user.
        /// Resolves UserId → EmployeeId, then queries LocationTracking for the server's current date.
        /// GET: api/locationtracking/today?userId=8
        /// </summary>
        [HttpGet("today")]
        [ProducesResponseType(typeof(TodayLocationTrackingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTodayPath([FromQuery] TodayLocationTrackingRequest request)
        {
            try
            {
                var currentUserId = CurrentUserId;
                if (!currentUserId.HasValue || currentUserId.Value <= 0)
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = AuthMessages.InvalidAuthenticationToken
                    });
                }

                if (request == null || request.UserId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = LocationTrackingMessages.UserIdRequired
                    });
                }

                int validatedUserId;
                try
                {
                    validatedUserId = GetValidatedUserId(request.UserId);
                }
                catch (TenantAccessException)
                {
                    return UserAccessDenied();
                }

                var (success, message, data) = await _locationTrackingService.GetTodayPathAsync(
                    validatedUserId,
                    CurrentOrganisationId);

                if (!success)
                {
                    if (message == LocationTrackingMessages.UserNotFound
                        || message == LocationTrackingMessages.EmployeeNotFoundForUser)
                    {
                        return NotFound(new { success = false, message });
                    }

                    return BadRequest(new { success = false, message });
                }

                return Ok(data);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.LocationTracking.GetTodayPath,
                    nameof(GetTodayPath),
                    ex,
                    CurrentUserId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message = GeneralMessages.SomethingWentWrongContactAdmin
                    });
            }
        }
    }
}
