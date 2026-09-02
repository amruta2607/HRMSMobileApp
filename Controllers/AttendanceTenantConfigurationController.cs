using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Controllers
{
    [Route("api/attendance")]
    [ApiController]
    [Authorize]
    public class AttendanceTenantConfigurationController : TenantBaseController
    {
        private readonly ITenantConfigurationRepository _tenantConfigurationRepository;

        public AttendanceTenantConfigurationController(
            ITenantConfigurationRepository tenantConfigurationRepository,
            ITenantContext tenantContext,
            ILogger<AttendanceTenantConfigurationController> logger)
            : base(tenantContext, logger)
        {
            _tenantConfigurationRepository = tenantConfigurationRepository;
        }

        /// <summary>
        /// Returns tenant-level attendance configuration for the logged-in user's organisation.
        /// GET: api/attendance/tenant-configuration
        /// </summary>
        [HttpGet("tenant-configuration")]
        public async Task<IActionResult> GetTenantConfiguration()
        {
            try
            {
                var tenantId = TenantContext.OrganisationId;
                if (!tenantId.HasValue || tenantId.Value <= 0)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = TenantAccessMessages.UserNotAuthenticated
                    });
                }

                Logger.LogInformation(
                    "Fetching attendance tenant configuration for organisation {OrganisationId}",
                    tenantId.Value);

                var row = await _tenantConfigurationRepository
                    .GetAttendanceTenantConfigurationByTenantIdAsync(tenantId.Value);

                return Ok(new AttendanceTenantConfigurationResponse
                {
                    IsPunchAllowedOnHolidayAndWeekend = row?.IsPunchAllowedOnHolidayAndWeekend ?? false
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching attendance tenant configuration.");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin
                });
            }
        }
    }
}
