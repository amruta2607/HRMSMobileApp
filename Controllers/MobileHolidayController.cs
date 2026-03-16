using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Controllers
{
    [Route("api/mobile/holidays")]
    [ApiController]
    [Authorize]
    public class MobileHolidayController : TenantBaseController
    {
        private readonly IMobileDashboardService _mobileDashboardService;

        public MobileHolidayController(
            IMobileDashboardService mobileDashboardService,
            ITenantContext tenantContext,
            ILogger<MobileHolidayController> logger)
            : base(tenantContext, logger)
        {
            _mobileDashboardService = mobileDashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<HolidayDto>>> Get()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _mobileDashboardService.GetLatestHolidaysAsync(tenantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching latest holidays for mobile dashboard.");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }
    }
}

