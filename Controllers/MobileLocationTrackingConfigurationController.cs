using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Services;

namespace MobileWebApi.Controllers
{
    [Route("api/mobile/locationtrackingconfiguration")]
    [ApiController]
    [Authorize]
    public class MobileLocationTrackingConfigurationController : TenantBaseController
    {
        private readonly ILocationTrackingConfigurationService _configurationService;

        public MobileLocationTrackingConfigurationController(
            ILocationTrackingConfigurationService configurationService,
            ITenantContext tenantContext,
            ILogger<MobileLocationTrackingConfigurationController> logger)
            : base(tenantContext, logger)
        {
            _configurationService = configurationService;
        }

        /// <summary>
        /// Returns location tracking configuration for the authenticated employee.
        /// GET: api/mobile/locationtrackingconfiguration
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetConfiguration()
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

                var (success, message, data) = await _configurationService.GetConfigurationAsync(
                    currentUserId.Value,
                    CurrentOrganisationId);

                if (!success)
                {
                    if (message == LocationTrackingConfigurationMessages.EmployeeNotFound
                        || message == LocationTrackingConfigurationMessages.TenantNotFound
                        || message == LocationTrackingConfigurationMessages.TenantConfigurationNotFound
                        || message == LocationTrackingConfigurationMessages.LocationTrackingConfigurationNotFound)
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
                    ExceptionCodes.LocationTrackingConfiguration.GetConfiguration,
                    nameof(GetConfiguration),
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
