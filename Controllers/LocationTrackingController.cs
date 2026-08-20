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
		/// Receives GPS coordinates from the mobile app and stores them in LocationTracking.
		/// POST: api/locationtracking
		/// </summary>
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
						return Unauthorized(new LocationTrackingResponse
						{
							Success = false,
							Message = AuthMessages.InvalidAuthenticationToken
						});
					}

					if (request.user_id <= 0)
					{
						return BadRequest(new LocationTrackingResponse
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

					var result = await _locationTrackingService.RecordLocationBatchAsync(
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
						ExceptionCodes.LocationTracking.RecordLocationBatch,
						nameof(RecordLocationBatch),
						ex,
						CurrentUserId);

					return StatusCode(
						StatusCodes.Status500InternalServerError,
						new LocationTrackingBatchResponse
						{
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
			/// <summary>
			/// Returns today's complete location tracking path for the employee linked to the given UserId.
			/// GET: /apipunch/location-tracking/today?UserId=8
			/// </summary>
        [HttpGet("/apipunch/location-tracking/today")]
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

                request ??= new TodayLocationTrackingRequest();

                if (request.UserId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = LocationTrackingMessages.UserIdRequired
                    });
                }

                // Regular users may only request their own path; HR/TenantAdmin may request others in-tenant.
                if (!HasElevatedAccess && request.UserId != currentUserId.Value)
                {
                    Logger.LogWarning(
                        LogMessages.TenantAccess.UserAccessViolation,
                        currentUserId.Value,
                        request.UserId);
                    return UserAccessDenied();
                }

                var (success, message, data) = await _locationTrackingService.GetTodayPathAsync(
                    request.UserId,
                    CurrentOrganisationId);

                if (!success)
                {
                    if (message == LocationTrackingMessages.UserNotFound
                        || message == LocationTrackingMessages.EmployeeNotAssociatedWithUser
                        || message == LocationTrackingMessages.EmployeeNotFound)
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
