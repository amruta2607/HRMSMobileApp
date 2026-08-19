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
    /// Provides tenant-scoped CRUD operations for asset maintenance records.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/AssetMaintenance")]
    public class AssetMaintenanceController : TenantBaseController
    {
        private readonly IAssetMaintenanceService _assetMaintenanceService;
        private readonly IUserRepository _userRepository;

        public AssetMaintenanceController(
            IAssetMaintenanceService assetMaintenanceService,
            IUserRepository userRepository,
            ITenantContext tenantContext,
            ILogger<AssetMaintenanceController> logger)
            : base(tenantContext, logger)
        {
            _assetMaintenanceService = assetMaintenanceService
                ?? throw new ArgumentNullException(nameof(assetMaintenanceService));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Creates a new asset maintenance record for the authenticated user's organisation.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] AssetMaintenanceRequest request)
        {
            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                var result = await _assetMaintenanceService.CreateAssetMaintenanceAsync(request);
                return StatusCode(StatusCodes.Status201Created, result);
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
                    ExceptionCodes.AssetMaintenance.Create,
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
        /// Returns a paged, searchable and sortable list of asset maintenance records
        /// for the authenticated user's organisation.
        /// </summary>
        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //public async Task<IActionResult> GetAll([FromQuery] AssetMaintenanceQueryParameters query)
        //{
        //    try
        //    {
        //        _ = CurrentOrganisationId;
        //        _ = CurrentUserId;

        //        var result = await _assetMaintenanceService.GetAllAssetMaintenanceAsync(query ?? new AssetMaintenanceQueryParameters());
        //        return Ok(result);
        //    }
        //    catch (TenantAccessException)
        //    {
        //        return TenantAccessDenied();
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException(
        //            ExceptionCodes.AssetMaintenance.GetList,
        //            nameof(GetAll),
        //            ex,
        //            CurrentUserId);

        //        return StatusCode(StatusCodes.Status500InternalServerError, new
        //        {
        //            message = GeneralMessages.UnexpectedError
        //        });
        //    }
        //}

        /// <summary>
        /// Updates an existing asset maintenance record for the authenticated user's organisation.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAssetMaintenanceRequest request)
        {
            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                var result = await _assetMaintenanceService.UpdateAssetMaintenanceAsync(id, request);
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
                    ExceptionCodes.AssetMaintenance.Update,
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
        /// Returns the maintenance history for a specific asset, scoped to the authenticated user's organisation.
        /// Returns an empty list with a success response when the asset has no maintenance records.
        /// </summary>
        [HttpGet("asset/{assetId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByAssetId(int assetId)
        {
            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                var result = await _assetMaintenanceService.GetAssetMaintenanceByAssetIdAsync(assetId);
                return Ok(result);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.AssetMaintenance.GetByAsset,
                    nameof(GetByAssetId),
                    ex,
                    CurrentUserId);

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    message = GeneralMessages.UnexpectedError
                });
            }
        }

        /// <summary>
        /// Deletes an asset maintenance record. Requires Admin or SuperAdmin work role.
        /// </summary>
        /// <response code="200">Maintenance record deleted successfully.</response>
        /// <response code="403">Caller does not have Admin or SuperAdmin role.</response>
        /// <response code="404">Maintenance record not found.</response>
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
                    Logger.LogWarning(LogMessages.AssetMaintenance.DeleteForbidden, userId, id);
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        success = false,
                        message = AssetMessages.MaintenanceDeleteForbidden
                    });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var result = await _assetMaintenanceService.DeleteAssetMaintenanceAsync(id, ipAddress);
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
                    ExceptionCodes.AssetMaintenance.Delete,
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
