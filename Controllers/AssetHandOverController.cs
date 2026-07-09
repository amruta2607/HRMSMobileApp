using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Repositories.Interfaces;
using MobileWebApi.Services;

namespace MobileWebApi.Controllers
{
    /// <summary>
    /// Provides tenant-specific asset hand over data for the mobile application.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/assethandover")]
    public class AssetHandOverController : TenantBaseController
    {
        private readonly IAssetHandOverRepository _assetHandOverRepository;

        public AssetHandOverController(
            IAssetHandOverRepository assetHandOverRepository,
            ITenantContext tenantContext,
            ILogger<AssetHandOverController> logger)
            : base(tenantContext, logger)
        {
            _assetHandOverRepository = assetHandOverRepository
                ?? throw new ArgumentNullException(nameof(assetHandOverRepository));
        }

        /// <summary>
        /// Returns all asset hand over records for the authenticated user's organisation.
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                var result = await _assetHandOverRepository.GetListAsync();
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.AssetHandOver.GetList,
                    nameof(GetList),
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
