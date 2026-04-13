using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LeaveController : TenantBaseController
    {
        private readonly ILeaveService _leaveService;
        private readonly IMobileModuleAccessService _mobileModuleAccessService;

        public LeaveController(
            ILeaveService leaveService, 
            IMobileModuleAccessService mobileModuleAccessService,
            ITenantContext tenantContext,
            ILogger<LeaveController> logger)
            : base(tenantContext, logger)
        {
            _leaveService = leaveService;
            _mobileModuleAccessService = mobileModuleAccessService;
        }

        private async Task<IActionResult?> EnsureLeaveAccessAsync(int tenantId)
        {
            var hasAccess = await _mobileModuleAccessService.HasAccess(tenantId, "Leave");
            if (hasAccess)
                return null;

            return StatusCode(403, new { Success = false, Message = "Leave module access is disabled for this tenant." });
        }

        /// <summary>
        /// Submit a new leave request
        /// POST: api/leave/request
        /// </summary>
        [HttpPost("request")]
        public async Task<IActionResult> CreateLeaveRequest([FromBody] LeaveRequestCreateRequest request)
        {
            var accessDenied = await EnsureLeaveAccessAsync(CurrentOrganisationId);
            if (accessDenied != null) return accessDenied;

            Logger.LogInformation(LogMessages.Leave.CreatingLeaveRequest, request.user);
            var result = await _leaveService.CreateLeaveRequestAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

		[HttpGet("/apipunch/leave/request/get")]
		public async Task<IActionResult> GetLeaveRequests(
	[FromQuery] int? user_id = null,
	[FromQuery] int? organization_id = null
)
		{
			// Validate tenant access - use user's org if not specified
			var validatedOrgId = GetValidatedOrganisationId(organization_id);
            var accessDenied = await EnsureLeaveAccessAsync(validatedOrgId);
            if (accessDenied != null) return accessDenied;

			// Validate user access - regular users can only see their own leave requests
			int? validatedUserId;
			try
			{
				validatedUserId = GetValidatedUserId(user_id);
			}
			catch (Services.TenantAccessException)
			{
				return UserAccessDenied();
			}

			Logger.LogInformation(
				LogMessages.Leave.FetchingLeaveRequestsByFilter,
				validatedUserId,
				validatedOrgId
			);

			var request = new LeaveRequestGetRequest
			{
				organization = validatedOrgId,
				user = validatedUserId
			};

			var result = await _leaveService.GetLeaveRequestsAsync(request);

			if (result.Success)
			{
				return Ok(result);
			}

			return BadRequest(result);
		}

		/// <summary>
		/// Approve a leave request
		/// PUT: api/leave/approve
		/// </summary>
		[HttpPut("approve")]
        public async Task<IActionResult> ApproveLeaveRequest([FromBody] ApproveLeaveRequest request)
        {
            var accessDenied = await EnsureLeaveAccessAsync(CurrentOrganisationId);
            if (accessDenied != null) return accessDenied;

            if (request == null)
            {
                Logger.LogWarning(GeneralMessages.RequestBodyCannotBeNull);
                return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
            }

            if (request.Id <= 0)
            {
                Logger.LogWarning(LeaveMessages.LeaveRequestIdRequired);
                return BadRequest(new { Success = false, Message = LeaveMessages.LeaveRequestIdRequired });
            }

            Logger.LogInformation(LogMessages.Leave.ApprovingLeaveRequest, request.Id);
            var result = await _leaveService.ApproveLeaveRequestAsync(request.Id, request.ApproverUserId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Reject a leave request
        /// PUT: api/leave/reject
        /// </summary>
        [HttpPut("reject")]
        public async Task<IActionResult> RejectLeaveRequest([FromBody] RejectLeaveRequest request)
        {
            var accessDenied = await EnsureLeaveAccessAsync(CurrentOrganisationId);
            if (accessDenied != null) return accessDenied;

            if (request == null)
            {
                Logger.LogWarning(GeneralMessages.RequestBodyCannotBeNull);
                return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
            }

            if (request.Id <= 0)
            {
                Logger.LogWarning(LeaveMessages.LeaveRequestIdRequired);
                return BadRequest(new { Success = false, Message = LeaveMessages.LeaveRequestIdRequired });
            }

            Logger.LogInformation(LogMessages.Leave.RejectingLeaveRequest, request.Id);
            var result = await _leaveService.RejectLeaveRequestAsync(request.Id, request.RejecterUserId, request.Reason);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
		/// <summary>
		/// Withdraw a leave request (only if pending)
		/// PUT: api/leave/withdraw
		/// </summary>
		[HttpPut("withdraw")]
		public async Task<IActionResult> WithdrawLeaveRequest([FromBody] WithdrawLeaveRequest request)
		{
            var accessDenied = await EnsureLeaveAccessAsync(CurrentOrganisationId);
            if (accessDenied != null) return accessDenied;

			if (request == null || request.Id <= 0)
			{
				return BadRequest(new
				{
					Success = false,
                    Message = LeaveMessages.LeaveRequestIdRequired
				});
			}

			var loggedInUserId = CurrentUserId;
			if (!loggedInUserId.HasValue)
			{
				return Unauthorized(new { Success = false, Message = TenantAccessMessages.UserNotAuthenticated });
			}

			// Do not trust client-provided userId; enforce it matches authenticated user
			if (request.UserId != loggedInUserId.Value)
			{
				Logger.LogWarning(LogMessages.TenantAccess.UnauthorizedUpdatePersonalDetails, loggedInUserId.Value, request.UserId);
				return UserAccessDenied();
			}

            Logger.LogInformation(LogMessages.Leave.CancellingLeaveRequest, request.Id);

			var result = await _leaveService.WithdrawLeaveRequestAsync(
				request.Id,
				loggedInUserId.Value,
				request.Reason
			);

			if (result.Success)
				return Ok(result);

			// If the service denied access (leave doesn't belong to the logged-in user), return 403
			if (string.Equals(result.Message, TenantAccessMessages.UserAccessDeniedSimple, System.StringComparison.OrdinalIgnoreCase))
				return UserAccessDenied();

			return BadRequest(result);
		}

		/// <summary>
		/// Get leave history (summary per request) for the logged-in user
		/// GET: api/leave/history
		/// </summary>
		[HttpGet("history")]
		public async Task<IActionResult> GetLeaveHistory()
		{
            var accessDenied = await EnsureLeaveAccessAsync(CurrentOrganisationId);
            if (accessDenied != null) return accessDenied;

			var userId = CurrentUserId;
			if (!userId.HasValue)
			{
                return Unauthorized(new { Success = false, Message = TenantAccessMessages.UserNotAuthenticated });
			}

            Logger.LogInformation(LogMessages.Leave.FetchingLeaveHistory, userId.Value);
			var result = await _leaveService.GetLeaveHistorySummaryAsync(userId.Value);

			if (result.Success)
			{
				return Ok(result);
			}

			return BadRequest(result);
		}

		/// <summary>
		/// Get leave balance for an employee
		/// GET: api/leave/balance/?user=10
		/// Note: Regular users can only see their own leave balance. HR/TenantAdmin can see all.
		/// </summary>
		[HttpGet("balance")]
        public async Task<IActionResult> GetLeaveBalance([FromQuery] int user, [FromQuery] int? organization = null)
        {
            // Validate tenant access - use user's org if not specified
            var validatedOrgId = GetValidatedOrganisationId(organization);
            var accessDenied = await EnsureLeaveAccessAsync(validatedOrgId);
            if (accessDenied != null) return accessDenied;
            
            // Validate user access - regular users can only see their own data
            int validatedUserId;
            try
            {
                validatedUserId = GetValidatedUserId(user);
            }
            catch (Services.TenantAccessException)
            {
                return UserAccessDenied();
            }
            
            Logger.LogInformation(LogMessages.Leave.FetchingLeaveBalance, validatedUserId);
            var result = await _leaveService.GetLeaveBalanceAsync(validatedUserId, validatedOrgId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
