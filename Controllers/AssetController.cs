using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Requests;
using MobileWebApi.Repositories.Interfaces;
using MobileWebApi.Services;

namespace MobileWebApi.Controllers
{
    /// <summary>
    /// Provides tenant-specific asset data for the mobile application.
    /// </summary>
    [ApiController]
    [Route("api/asset")]
    [Authorize]
    public class AssetController : TenantBaseController
    {
        private readonly IAssetRepository _assetRepository;

        public AssetController(
            IAssetRepository assetRepository,
            ITenantContext tenantContext,
            ILogger<AssetController> logger)
            : base(tenantContext, logger)
        {
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
        }

        /// <summary>
        /// Returns all assets for the authenticated user's organisation.
        /// </summary>
        [HttpGet("list")]
        public async Task<IActionResult> GetAssets()
        {
            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                var result = await _assetRepository.GetAssetsAsync();
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Asset.GetList,
                    nameof(GetAssets),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Creates a new asset for the authenticated user's organisation.
        /// </summary>
        [HttpPost("Add-Asset")]
        public async Task<IActionResult> Create([FromBody] CreateAssetRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = AssetMessages.RequestBodyCannotBeNull });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                var result = await _assetRepository.CreateAssetAsync(request);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (AssetValidationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Asset.Create,
                    nameof(Create),
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
