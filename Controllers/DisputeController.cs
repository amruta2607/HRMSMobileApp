using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Controllers
{
    [Route("api/disputes")]
    [ApiController]
    [Authorize]
    public class DisputeController : TenantBaseController
    {
        private readonly IDisputeService _disputeService;

        public DisputeController(
            IDisputeService disputeService,
            ITenantContext tenantContext,
            ILogger<DisputeController> logger)
            : base(tenantContext, logger)
        {
            _disputeService = disputeService;
        }

        /// <summary>
        /// Get all active dispute categories
        /// GET: /api/disputes/categories
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> GetDisputeCategories()
        {
            Logger.LogInformation(LogMessages.Dispute.FetchingDisputeCategories);
            
            var result = await _disputeService.GetDisputeCategoriesAsync();
            
            if (result.Success)
            {
                return Ok(result);
            }
            
            return BadRequest(result);
        }

        /// <summary>
        /// Submit a dispute request for the logged-in user.
        /// UserId, EmployeeId, and TenantId are taken from the authenticated context — not from the body.
        /// POST: /api/disputes
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SubmitDispute([FromBody] DisputeSubmitRequest request)
        {
            if (request == null)
            {
                return BadRequest(new DisputeSubmitResponse
                {
                    Success = false,
                    Message = GeneralMessages.RequestBodyCannotBeNull,
                    Data = null
                });
            }

            var userId = CurrentUserId;
            if (!userId.HasValue || userId.Value <= 0)
            {
                return Unauthorized(new DisputeSubmitResponse
                {
                    Success = false,
                    Message = TenantAccessMessages.UserNotAuthenticated,
                    Data = null
                });
            }

            // Validate DisputeCategoryId
            if (request.DisputeCategoryId <= 0)
            {
                return BadRequest(new DisputeSubmitResponse
                {
                    Success = false,
                    Message = DisputeMessages.DisputeCategoryIdRequired,
                    Data = null
                });
            }

            var tenantId = CurrentOrganisationId;

            Logger.LogInformation(LogMessages.Dispute.SubmittingDispute, userId.Value);
            
            var result = await _disputeService.SubmitDisputeAsync(request, userId.Value, tenantId);
            
            if (result.Success)
            {
                return StatusCode(201, result);
            }
            
            return BadRequest(result);
        }
    }
}
