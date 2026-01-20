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
    public class AlertController : TenantBaseController
    {
        private readonly IAlertService _service;

        public AlertController(
            IAlertService service, 
            ITenantContext tenantContext,
            ILogger<AlertController> logger)
            : base(tenantContext, logger)
        {
            _service = service;
        }

   
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAlertsByUserId(
            int userId, 
            [FromQuery] bool? isRead = null, 
            [FromQuery] bool? isActive = null)
        {
            Logger.LogInformation(LogMessages.Alert.RetrievingAlertsForUser, userId);
            var result = await _service.GetAlertsByUserIdAsync(userId, isRead, isActive);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

 
        [HttpPut("mark-read")]
        public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
        {
            if (request == null)
            {
                Logger.LogWarning(GeneralMessages.RequestBodyCannotBeNull);
                return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
            }

            if (request.Id <= 0)
            {
                Logger.LogWarning(LogMessages.Controller.InvalidAlertId);
                return BadRequest(new { Success = false, Message = AlertMessages.AlertIdRequired });
            }

            Logger.LogInformation(LogMessages.Alert.MarkingAlertAsRead, request.Id);
            var result = await _service.MarkAsReadAsync(request.Id, request.UpdateUserId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        
   
        [HttpPut("approve-request")]
        public async Task<IActionResult> ApproveRequestFromAlert([FromBody] ApproveRequestFromAlertRequest request)
        {
            if (request == null)
            {
                Logger.LogWarning(GeneralMessages.RequestBodyCannotBeNull);
                return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
            }

            if (request.AlertId <= 0)
            {
                Logger.LogWarning(LogMessages.Controller.InvalidAlertId);
                return BadRequest(new { Success = false, Message = AlertMessages.AlertIdRequired });
            }

            var approverUserId = CurrentUserId ?? 0;
            Logger.LogInformation(LogMessages.Alert.ApprovingRequestFromAlert, request.AlertId);
            
            var result = await _service.ApproveRequestFromAlertAsync(request, approverUserId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

   
        [HttpPut("reject-request")]
        public async Task<IActionResult> RejectRequestFromAlert([FromBody] RejectRequestFromAlertRequest request)
        {
            if (request == null)
            {
                Logger.LogWarning(GeneralMessages.RequestBodyCannotBeNull);
                return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
            }

            if (request.AlertId <= 0)
            {
                Logger.LogWarning(LogMessages.Controller.InvalidAlertId);
                return BadRequest(new { Success = false, Message = AlertMessages.AlertIdRequired });
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { Success = false, Message = AlertMessages.RejectionReasonRequired });
            }

            var rejecterUserId = CurrentUserId ?? 0;
            Logger.LogInformation(LogMessages.Alert.RejectingRequestFromAlert, request.AlertId);
            
            var result = await _service.RejectRequestFromAlertAsync(request, rejecterUserId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

    }
}
