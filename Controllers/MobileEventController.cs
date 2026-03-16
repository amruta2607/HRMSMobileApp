using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Controllers
{
    [Route("api/mobile/events")]
    [ApiController]
    [Authorize]
    public class MobileEventController : TenantBaseController
    {
        private readonly IMobileDashboardService _mobileDashboardService;

        public MobileEventController(
            IMobileDashboardService mobileDashboardService,
            ITenantContext tenantContext,
            ILogger<MobileEventController> logger)
            : base(tenantContext, logger)
        {
            _mobileDashboardService = mobileDashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> Get()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _mobileDashboardService.GetLatestEventsAsync(tenantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LogMessages.MobileDashboard.ErrorFetchingLatestEvents);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }
    }
}

