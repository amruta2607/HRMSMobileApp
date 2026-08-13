using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Services;

namespace MobileWebApi.Controllers
{
    /// <summary>
    /// Tenant-scoped HR dashboard widgets: recent birthdays, work anniversaries, and awards.
    /// </summary>
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : TenantBaseController
    {
        private readonly IMobileDashboardService _mobileDashboardService;

        public DashboardController(
            IMobileDashboardService mobileDashboardService,
            ITenantContext tenantContext,
            ILogger<DashboardController> logger)
            : base(tenantContext, logger)
        {
            _mobileDashboardService = mobileDashboardService
                ?? throw new ArgumentNullException(nameof(mobileDashboardService));
        }

        /// <summary>
        /// Returns active employees in the current tenant whose birthday is today or within the next 4 days
        /// (visible from 4 days before the birthday through the birthday date).
        /// </summary>
        [HttpGet("birthdays")]
        [ProducesResponseType(typeof(IEnumerable<BirthdayDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBirthdays()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _mobileDashboardService.GetBirthdaysAsync(tenantId);
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LogMessages.MobileDashboard.ErrorFetchingBirthdays);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Returns active employees in the current tenant whose work anniversary is today or within the next 4 days
        /// (visible from 4 days before the anniversary through the anniversary date).
        /// </summary>
        [HttpGet("work-anniversaries")]
        [ProducesResponseType(typeof(IEnumerable<WorkAnniversaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkAnniversaries()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _mobileDashboardService.GetWorkAnniversariesAsync(tenantId);
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LogMessages.MobileDashboard.ErrorFetchingWorkAnniversaries);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Returns awards in the current tenant whose award date is today or within the next 4 days
        /// (visible from 4 days before the award date through the award date).
        /// </summary>
        [HttpGet("awards")]
        [ProducesResponseType(typeof(IEnumerable<AwardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAwards()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _mobileDashboardService.GetAwardsAsync(tenantId);
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LogMessages.MobileDashboard.ErrorFetchingAwards);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }
    }
}
