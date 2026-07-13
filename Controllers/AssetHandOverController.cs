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
    }
}
