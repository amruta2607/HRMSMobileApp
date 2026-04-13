using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;

namespace MobileWebApi.Controllers
{
    [Route("api/mobile/module-access")]
    [ApiController]
    [Authorize]
    public class MobileModuleAccessController : TenantBaseController
    {
        private readonly IMobileModuleAccessService _accessService;

        public MobileModuleAccessController(
            IMobileModuleAccessService accessService,
            ITenantContext tenantContext,
            ILogger<MobileModuleAccessController> logger)
            : base(tenantContext, logger)
        {
            _accessService = accessService;
        }

        [HttpGet("{tenantId:int}")]
        public async Task<IActionResult> GetModuleAccess([FromRoute] int tenantId)
        {
            var validatedTenantId = GetValidatedOrganisationId(tenantId);
            var dto = await _accessService.GetModuleAccess(validatedTenantId);
            return Ok(dto);
        }
    }
}

