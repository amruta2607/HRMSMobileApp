using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Controllers
{
    /// <summary>
    /// API endpoints for punch tracking timeline operations.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/punch")]
    public class PunchTrackingController : TenantBaseController
    {
        private readonly IPunchTrackingRepository _punchTrackingRepository;

        public PunchTrackingController(
            IPunchTrackingRepository punchTrackingRepository,
            ITenantContext tenantContext,
            ILogger<PunchTrackingController> logger)
            : base(tenantContext, logger)
        {
            _punchTrackingRepository = punchTrackingRepository;
        }

        /// <summary>
        /// Returns all punch in/out events for a specific attendance day.
        /// GET: /api/punch/tracking/{punchId}
        /// </summary>
        /// <param name="punchId">The punch record identifier.</param>
        [HttpGet("tracking/{punchId}")]
        public async Task<IActionResult> GetTimeline(int punchId)
        {
            try
            {
                if (punchId <= 0)
                {
                    return BadRequest(new { Success = false, Message = "PunchId is required." });
                }

                var result = await _punchTrackingRepository.GetPunchTrackingTimelineAsync(punchId);

                return result.Status switch
                {
                    PunchTrackingTimelineStatus.Unauthorized => Unauthorized(new
                    {
                        Success = false,
                        Message = TenantAccessMessages.UserNotAuthenticated
                    }),
                    PunchTrackingTimelineStatus.Forbidden => StatusCode(403, new
                    {
                        Success = false,
                        Message = TenantAccessMessages.UserAccessDenied
                    }),
                    PunchTrackingTimelineStatus.NotFound => NotFound(new
                    {
                        Success = false,
                        Message = AttendanceMessages.AttendanceNotFound
                    }),
                    _ => Ok(result.Data)
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error fetching punch tracking timeline for PunchId {PunchId}", punchId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }
    }
}
