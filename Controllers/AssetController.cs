using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Requests;
using MobileWebApi.Models.Responses;
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
        private readonly IUserRepository _userRepository;

        public AssetController(
            IAssetRepository assetRepository,
            IUserRepository userRepository,
            ITenantContext tenantContext,
            ILogger<AssetController> logger)
            : base(tenantContext, logger)
        {
            _assetRepository = assetRepository ?? throw new ArgumentNullException(nameof(assetRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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
        /// Returns all lookup values required by the Create Asset screen.
        /// </summary>
        [HttpGet("lookups")]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var userId = CurrentUserId;

                Logger.LogInformation(LogMessages.Asset.FetchingLookups, userId, tenantId);

                var result = await _assetRepository.GetLookupsAsync();
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Asset.GetLookups,
                    nameof(GetLookups),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
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
                return BadRequest(new { success = false, message = AssetMessages.RequestBodyCannotBeNull });
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
                return BadRequest(new { success = false, message = ex.Message });
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

        /// <summary>
        /// Updates editable asset information for the authenticated user's organisation.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { success = false, message = AssetMessages.RequestBodyCannotBeNull });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                var result = await _assetRepository.UpdateAssetAsync(id, request);
                return Ok(result);
            }
            catch (AssetNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (AssetValidationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Asset.Update,
                    nameof(Update),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Returns AssetHistory rows for the specified asset where SourceTable = 'Asset',
        /// ordered by ActionDate descending (latest activity first).
        /// </summary>
        /// <param name="assetId">The asset identifier.</param>
        /// <response code="200">Timeline fetched successfully (may be an empty list).</response>
        /// <response code="401">Caller is not authenticated or tenant access is denied.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpGet("{assetId:int}/timeline")]
        [ProducesResponseType(typeof(AssetTimelineListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTimeline(int assetId)
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var userId = CurrentUserId;

                Logger.LogInformation(
                    LogMessages.Asset.FetchingTimeline,
                    assetId,
                    userId,
                    tenantId);

                var result = await _assetRepository.GetAssetTimelineAsync(assetId);
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Asset.GetTimeline,
                    nameof(GetTimeline),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Returns the QR code for the specified asset.
        /// Returns the QRCodePath value as stored on the Asset record.
        /// </summary>
        /// <response code="200">QR code retrieved successfully.</response>
        /// <response code="404">Asset or QR code not found.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpGet("{assetId:int}/QrCode")]
        [ProducesResponseType(typeof(AssetQrCodeResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetQrCode(int assetId)
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var userId = CurrentUserId;

                Logger.LogInformation(
                    LogMessages.Asset.FetchingQrCode,
                    assetId,
                    userId,
                    tenantId);

                var result = await _assetRepository.GetAssetQrCodeAsync(assetId);

                Logger.LogInformation(
                    LogMessages.Asset.QrCodeFetched,
                    assetId,
                    tenantId);

                return Ok(result);
            }
            catch (AssetNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (AssetQrCodeNotFoundException ex)
            {
                Logger.LogWarning(
                    LogMessages.Asset.QrCodeNotFound,
                    assetId,
                    CurrentOrganisationId);

                return NotFound(new { success = false, message = ex.Message });
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Asset.GetQrCode,
                    nameof(GetQrCode),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Deletes an asset and all dependent records. Requires Admin or SuperAdmin work role.
        /// </summary>
        /// <response code="200">Asset deleted successfully.</response>
        /// <response code="403">Caller does not have Admin or SuperAdmin role.</response>
        /// <response code="404">Asset not found.</response>
        /// <response code="500">Unexpected server error.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = CurrentUserId
                    ?? throw new TenantAccessException(TenantAccessMessages.UserNotAuthenticated);

                if (!await HasAdminOrSuperAdminAccessAsync(userId))
                {
                    Logger.LogWarning(LogMessages.Asset.DeleteForbidden, userId, id);
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        success = false,
                        message = AssetMessages.DeleteForbidden
                    });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _assetRepository.DeleteAssetAsync(id, ipAddress);
                return Ok(result);
            }
            catch (AssetNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (TenantAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Asset.Delete,
                    nameof(Delete),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        private async Task<bool> HasAdminOrSuperAdminAccessAsync(int userId)
        {
            var roles = await _userRepository.GetActiveWorkRolesByUserIdAsync(userId);
            return WorkRoleHelper.IsAdminOrSuperAdmin(roles);
        }
    }
}
