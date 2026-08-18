using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Services;

namespace MobileWebApi.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize]
    public class DashboardController : TenantBaseController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(
            IDashboardService dashboardService,
            ITenantContext tenantContext,
            ILogger<DashboardController> logger)
            : base(tenantContext, logger)
        {
            _dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        }

        /// <summary>
        /// Returns active employees in the current tenant whose birthday falls today through today + 4 days.
        /// The original birth year is ignored; December/January year boundaries are included.
        /// </summary>
        [HttpGet("birthdays")]
        [ProducesResponseType(typeof(IEnumerable<DashboardBirthdayDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBirthdays()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _dashboardService.GetUpcomingBirthdaysAsync(tenantId);
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Dashboard.GetBirthdays,
                    nameof(GetBirthdays),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Returns active employees in the current tenant whose work anniversary falls today through today + 4 days.
        /// The original joining year is ignored; service years are calculated as of the anniversary date.
        /// </summary>
        [HttpGet("work-anniversaries")]
        [ProducesResponseType(typeof(IEnumerable<DashboardWorkAnniversaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorkAnniversaries()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _dashboardService.GetUpcomingWorkAnniversariesAsync(tenantId);
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Dashboard.GetWorkAnniversaries,
                    nameof(GetWorkAnniversaries),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Returns awards in the current tenant whose award date falls today through today + 4 days.
        /// </summary>
        [HttpGet("awards")]
        [ProducesResponseType(typeof(IEnumerable<DashboardAwardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAwards()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _dashboardService.GetUpcomingAwardsAsync(tenantId);
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Dashboard.GetAwards,
                    nameof(GetAwards),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }
    }
}
