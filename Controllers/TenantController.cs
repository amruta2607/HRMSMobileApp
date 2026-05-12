using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;

namespace MobileWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantController : TenantBaseController
    {
        private readonly ITenantConfigurationRepository _tenantConfigurationRepository;

        public TenantController(
            ITenantConfigurationRepository tenantConfigurationRepository,
            ITenantContext tenantContext,
            ILogger<TenantController> logger)
            : base(tenantContext, logger)
        {
            _tenantConfigurationRepository = tenantConfigurationRepository;
        }

        /// <summary>
        /// Returns TenantConfiguration for the logged-in user's organisation (TenantId / OrganisationId from JWT).
        /// GET: api/Tenant/GetCompanyLogo
        /// </summary>
        [HttpGet(nameof(GetCompanyLogo))]
        public async Task<IActionResult> GetCompanyLogo()
        {
            return await ExecuteWithTenantValidation(async () =>
            {
                int organisationId = CurrentOrganisationId;
                Logger.LogInformation(LogMessages.User.RetrievingCompanyLogoForOrganisation, organisationId);

                var row = await _tenantConfigurationRepository.GetTenantConfigurationRowByTenantIdAsync(organisationId);
                if (row == null)
                {
                    return NotFound(new
                    {
                        Success = false,
                        Message = TenantMessages.TenantConfigurationNotFound
                    });
                }

                return Ok(new
                {
                    Success = true,
                    Message = TenantMessages.CompanyLogoRetrievedSuccessfully,
                    Data = row
                });
            });
        }
    }
}
