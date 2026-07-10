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
    /// Provides scanner-based asset lookup for the mobile application.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/scanner")]
    public class ScannerController : TenantBaseController
    {
        private readonly IScannerRepository _scannerRepository;

        public ScannerController(
            IScannerRepository scannerRepository,
            ITenantContext tenantContext,
            ILogger<ScannerController> logger)
            : base(tenantContext, logger)
        {
            _scannerRepository = scannerRepository ?? throw new ArgumentNullException(nameof(scannerRepository));
        }

        /// <summary>
        /// Returns complete asset details for a scanned asset code, QR text, or asset number.
        /// </summary>
        [HttpGet("asset/{code}")]
        public async Task<IActionResult> GetAsset(string code)
        {
            try
            {
                _ = CurrentOrganisationId;
                _ = CurrentUserId;

                if (string.IsNullOrWhiteSpace(code))
                {
                    return BadRequest(new { message = ScannerMessages.CodeRequired });
                }

                var asset = await _scannerRepository.GetAssetAsync(code);
                if (asset == null)
                {
                    return NotFound(new { message = ScannerMessages.AssetNotFound });
                }

                return Ok(asset);
            }
            catch (TenantAccessException)
            {
                return TenantAccessDenied();
            }
            catch (Exception ex)
            {
                Logger.LogException(
                    ExceptionCodes.Scanner.GetAsset,
                    nameof(GetAsset),
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
