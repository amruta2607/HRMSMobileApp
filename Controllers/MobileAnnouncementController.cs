using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Controllers
{
    [Route("api/mobile/announcements")]
    [ApiController]
    [Authorize]
    public class MobileAnnouncementController : TenantBaseController
    {
        private readonly IMobileDashboardService _mobileDashboardService;

        public MobileAnnouncementController(
            IMobileDashboardService mobileDashboardService,
            ITenantContext tenantContext,
            ILogger<MobileAnnouncementController> logger)
            : base(tenantContext, logger)
        {
            _mobileDashboardService = mobileDashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AnnouncementDto>>> Get()
        {
            try
            {
                var organisationId = CurrentOrganisationId;
                var result = await _mobileDashboardService.GetLatestAnnouncementsAsync(organisationId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, LogMessages.MobileDashboard.ErrorFetchingLatestAnnouncements);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }
    }
}

