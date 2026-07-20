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
    /// Provides tenant-specific asset hand over data for the mobile application.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/assethandover")]
    public class AssetHandOverController : TenantBaseController
    {
        private readonly IAssetHandOverRepository _assetHandOverRepository;
        private readonly IUserRepository _userRepository;

        public AssetHandOverController(
            IAssetHandOverRepository assetHandOverRepository,
            IUserRepository userRepository,
            ITenantContext tenantContext,
            ILogger<AssetHandOverController> logger)
            : base(tenantContext, logger)
        {
            _assetHandOverRepository = assetHandOverRepository
                ?? throw new ArgumentNullException(nameof(assetHandOverRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
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

        /// <summary>
        /// Returns lookup data for the Asset HandOver screen.
        /// </summary>
        [HttpGet("/api/asset-handover/lookups")]
        public async Task<IActionResult> GetLookups()
        {
            try
            {
                var tenantId = CurrentOrganisationId;
                var userId = CurrentUserId;

                Logger.LogInformation(LogMessages.AssetHandOver.FetchingLookups, userId, tenantId);

                var result = await _assetHandOverRepository.GetLookupsAsync();
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.AssetHandOver.GetLookups,
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
        /// Creates a new asset handover record and updates the related asset assignment.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssetHandoverRequest request)
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

                var result = await _assetHandOverRepository.AssetHandoverAsync(request);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (AssetNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (AssetEmployeeNotFoundException ex)
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
                    ExceptionCodes.AssetHandOver.Handover,
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
        /// Updates an existing asset handover record for the authenticated user's organisation.
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetHandoverRequest request)
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

                var result = await _assetHandOverRepository.UpdateAssetHandoverAsync(id, request);
                return Ok(result);
            }
            catch (AssetHandoverNotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (AssetEmployeeNotFoundException ex)
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
                    ExceptionCodes.AssetHandOver.Update,
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
        /// Deletes an asset handover record. Requires Admin or SuperAdmin work role.
        /// </summary>
        /// <response code="200">Handover deleted successfully.</response>
        /// <response code="403">Caller does not have Admin or SuperAdmin role.</response>
        /// <response code="404">Handover not found.</response>
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
                    Logger.LogWarning(LogMessages.AssetHandOver.DeleteForbidden, userId, id);
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        success = false,
                        message = AssetMessages.DeleteForbidden
                    });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _assetHandOverRepository.DeleteAssetHandoverAsync(id, ipAddress);
                return Ok(result);
            }
            catch (AssetHandoverNotFoundException ex)
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
                    ExceptionCodes.AssetHandOver.Delete,
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
