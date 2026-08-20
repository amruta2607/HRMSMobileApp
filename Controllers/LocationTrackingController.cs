using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;
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
        private readonly ILocationTrackingIssueService _locationTrackingIssueService;

        public LocationTrackingController(
            ILocationTrackingService locationTrackingService,
            ILocationTrackingIssueService locationTrackingIssueService,
            ITenantContext tenantContext,
            ILogger<LocationTrackingController> logger)
            : base(tenantContext, logger)
        {
            _locationTrackingService = locationTrackingService;
            _locationTrackingIssueService = locationTrackingIssueService;
        }

        /// <summary>
        /// Returns today's complete location tracking path for the requested user.
        /// Resolves UserId → EmployeeId, then queries LocationTracking for the server's current date.
        /// GET: api/locationtracking/today?userId=8
        /// Receives GPS coordinates from the mobile app and stores them in LocationTracking.
        /// POST: api/locationtracking
        /// </summary>
        [HttpGet("today")]
        [ProducesResponseType(typeof(TodayLocationTrackingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTodayPath([FromQuery] TodayLocationTrackingRequest request)
        [HttpPost("/apipunch/location-tracking/add-location-tracking/")]
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
                    return Unauthorized(new
                    return Unauthorized(new LocationTrackingResponse
                    {
                        success = false,
                        message = AuthMessages.InvalidAuthenticationToken
                        Success = false,
                        Message = AuthMessages.InvalidAuthenticationToken
                    });
                }

                if (request == null || request.UserId <= 0)
                if (request.user_id <= 0)
                {
                    return BadRequest(new
                    return BadRequest(new LocationTrackingResponse
                    {
                        success = false,
                        message = LocationTrackingMessages.UserIdRequired
                        Success = false,
                        Message = LocationTrackingMessages.UserIdRequired
                    });
                }

                int validatedUserId;
                try
                if (request.user_id != currentUserId.Value)
                {
                    validatedUserId = GetValidatedUserId(request.UserId);
                    Logger.LogWarning(
                        LogMessages.TenantAccess.UserAccessViolation,
                        currentUserId.Value,
                        request.user_id);
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

        /// <summary>
        /// Receives multiple GPS coordinates from the mobile app for offline sync.
        /// POST: api/locationtracking/batch
        /// </summary>
        [HttpPost("/apipunch/location-tracking/add-batch-location/")]
        public async Task<IActionResult> RecordLocationBatch([FromBody] LocationTrackingBatchRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new LocationTrackingBatchResponse
                    {
                        Success = false,
                        Message = GeneralMessages.RequestBodyCannotBeNull
                    });
                }

                var currentUserId = CurrentUserId;
                if (!currentUserId.HasValue || currentUserId.Value <= 0)
                {
                    return Unauthorized(new LocationTrackingBatchResponse
                    {
                        Success = false,
                        Message = AuthMessages.InvalidAuthenticationToken
                    });
                }

                if (request.user_id <= 0)
                {
                    return BadRequest(new LocationTrackingBatchResponse
                    {
                        Success = false,
                        Message = LocationTrackingMessages.UserIdRequired
                    });
                }

                if (request.user_id != currentUserId.Value)
                {
                    Logger.LogWarning(
                        LogMessages.TenantAccess.UserAccessViolation,
                        currentUserId.Value,
                        request.user_id);
                    return UserAccessDenied();
                }

                var (success, message, data) = await _locationTrackingService.GetTodayPathAsync(
                    validatedUserId,
                var result = await _locationTrackingService.RecordLocationBatchAsync(
                    request,
                    currentUserId.Value,
                    CurrentOrganisationId);

                if (!success)
                if (!result.Success)
                {
                    if (message == LocationTrackingMessages.UserNotFound
                        || message == LocationTrackingMessages.EmployeeNotFoundForUser)
                    if (result.Message == LocationTrackingMessages.EmployeeNotFound
                        || result.Message == LocationTrackingMessages.TenantNotFound)
                    {
                        return NotFound(new { success = false, message });
                        return NotFound(result);
                    }

                    return BadRequest(new { success = false, message });
                    return BadRequest(result);
                }

                return Ok(data);
                return Ok(result);
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
                    ExceptionCodes.LocationTracking.RecordLocationBatch,
                    nameof(RecordLocationBatch),
                    ex,
                    CurrentUserId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    new LocationTrackingBatchResponse
                    {
                        success = false,
                        message = GeneralMessages.SomethingWentWrongContactAdmin
                        Success = false,
                        Message = GeneralMessages.SomethingWentWrongContactAdmin
                    });
            }
        }

        /// <summary>
        /// Logs a location tracking violation reported by the mobile application.
        /// POST: /apipunch/location-tracking/add-issue
        /// </summary>
        [HttpPost("/apipunch/location-tracking/add-issue")]
        public async Task<IActionResult> AddLocationTrackingIssue([FromBody] LocationTrackingIssueRequest request)
        {
            try
            {
                Logger.LogInformation(
                    LogMessages.LocationTrackingIssue.ApiRequestReceived,
                    CurrentUserId);

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

                Logger.LogInformation(
                    LogMessages.LocationTrackingIssue.AuthenticatedUser,
                    currentUserId.Value,
                    CurrentUsername);

                if (request.user_id != currentUserId.Value)
                {
                    Logger.LogWarning(
                        LogMessages.TenantAccess.UserAccessViolation,
                        currentUserId.Value,
                        request.user_id);
                    return UserAccessDenied();
                }

                var result = await _locationTrackingIssueService.AddLocationTrackingIssueAsync(
                    request,
                    currentUserId.Value,
                    CurrentOrganisationId);

                if (!result.Success)
                {
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
                    ExceptionCodes.LocationTracking.AddIssue,
                    nameof(AddLocationTrackingIssue),
                    ex,
                    CurrentUserId);

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new LocationTrackingResponse
                    {
                        Success = false,
                        Message = GeneralMessages.UnexpectedError
                    });
            }
        }
    }
}
