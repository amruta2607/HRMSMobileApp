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
        private readonly IEmployeeService _employeeService;

        public DisputeController(
            IDisputeService disputeService,
            IEmployeeService employeeService,
            ITenantContext tenantContext,
            ILogger<DisputeController> logger)
            : base(tenantContext, logger)
        {
            _disputeService = disputeService;
            _employeeService = employeeService;
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
        /// Submit a dispute request
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

            // Validate UserId
            if (request.UserId <= 0)
            {
                return BadRequest(new DisputeSubmitResponse
                {
                    Success = false,
                    Message = "UserId is required.",
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

            // Validate that user can only submit disputes for themselves (unless HR/TenantAdmin)
            if (!HasElevatedAccess)
            {
                if (request.UserId != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.Dispute.UserAttemptedSubmitDispute, 
                        CurrentUserId, request.UserId);
                    return UserAccessDenied();
                }
            }

            Logger.LogInformation(LogMessages.Dispute.SubmittingDispute, request.UserId);
            
            var result = await _disputeService.SubmitDisputeAsync(request, CurrentOrganisationId);
            
            if (result.Success)
            {
                return StatusCode(201, result);
            }
            
            return BadRequest(result);
        }
    }
}

