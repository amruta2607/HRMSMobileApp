using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Controllers
{
    [Route("api/mobile/training")]
    [ApiController]
    [Authorize]
    public class MobileTrainingController : TenantBaseController
    {
        private readonly IMobileDashboardService _mobileDashboardService;

        public MobileTrainingController(
            IMobileDashboardService mobileDashboardService,
            ITenantContext tenantContext,
            ILogger<MobileTrainingController> logger)
            : base(tenantContext, logger)
        {
            _mobileDashboardService = mobileDashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainingDto>>> Get()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var result = await _mobileDashboardService.GetLatestTrainingsAsync(tenantId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching latest trainings for mobile dashboard.");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }
    }
}

