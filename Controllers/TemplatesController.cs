using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Services;

namespace MobileWebApi.Controllers
{
    /// <summary>
    /// Provides template lookup endpoints for the mobile application.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/templates")]
    public class TemplatesController : TenantBaseController
    {
        private readonly ITemplateRepository _templateRepository;

        public TemplatesController(
            ITemplateRepository templateRepository,
            ITenantContext tenantContext,
            ILogger<TemplatesController> logger)
            : base(tenantContext, logger)
        {
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
        }

        /// <summary>
        /// Returns all active template names for the authenticated user's tenant.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTemplates()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                Logger.LogInformation(LogMessages.Template.FetchingTemplates, tenantId);

                var templates = await _templateRepository.GetTemplatesAsync();
                return Ok(templates);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Template.GetList,
                    nameof(GetTemplates),
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
