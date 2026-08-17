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
    /// Provides tenant-specific asset dashboard data for the mobile application.
    /// </summary>
    [ApiController]
    [Route("api/asset")]
    [Authorize]
    public class AssetDashboardController : TenantBaseController
    {
        private readonly IAssetDashboardRepository _assetDashboardRepository;

        public AssetDashboardController(
            IAssetDashboardRepository assetDashboardRepository,
            ITenantContext tenantContext,
            ILogger<AssetDashboardController> logger)
            : base(tenantContext, logger)
        {
            _assetDashboardRepository = assetDashboardRepository
                ?? throw new ArgumentNullException(nameof(assetDashboardRepository));
        }

        /// <summary>
        /// Returns the asset dashboard for the authenticated user's organisation.
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                var dashboard = await _assetDashboardRepository.GetDashboardAsync();
                return Ok(dashboard);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.AssetDashboard.GetDashboard,
                    nameof(GetDashboard),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Returns the asset dashboard filtered according to the authenticated user's highest work role.
        /// Same response shape as <c>GET api/asset/dashboard</c>.
        /// </summary>
        [HttpGet("role-dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRoleDashboard()
        {
            try
            {
                var dashboard = await _assetDashboardRepository.GetRoleDashboardAsync();
                return Ok(dashboard);
            }
            catch (TenantAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Dashboard.GetDashboard,
                    nameof(GetRoleDashboard),
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
