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

        [HttpGet("{organizationId:int}")]
        public async Task<IActionResult> GetModuleAccess([FromRoute] int organizationId)
        {
            var validatedOrganizationId = GetValidatedOrganisationId(organizationId);
            var dto = await _accessService.GetModuleAccess(validatedOrganizationId);
            return Ok(dto);
        }
    }
}

