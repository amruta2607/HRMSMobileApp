using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using MobileWebApi.Helper;

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


		[HttpGet("user")]
		public async Task<IActionResult> GetAlertsByUserId()
		{
			try
			{
				var userId = CurrentUserId ?? 0;
				Logger.LogInformation(LogMessages.Alert.RetrievingAlertsForUser, userId);
				// Fetch only unread alerts (IsRead == false)
				var result = await _service.GetAlertsByUserIdAsync(userId, isRead: false);
				if (result.Success && result.Data != null)
				{
					result.Data = result.Data
						.OrderByDescending(a => a.InsertDate ?? DateTime.MinValue)
						.ThenByDescending(a => a.Id)
						.ToList();
				}
				if (result.Success)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				Logger.LogException(ExceptionCodes.Alert.GetAlertsByUser, nameof(GetAlertsByUserId), ex, CurrentUserId);
				return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
			}
		}

		[HttpGet("user/count")]
		public async Task<IActionResult> GetUnreadAlertCountByUserId()
		{
			try
			{
				var userId = CurrentUserId ?? 0;
				Logger.LogInformation(LogMessages.Alert.RetrievingAlertsForUser, userId);

				var result = await _service.GetUnreadAlertCountByUserIdAsync(userId);
				if (result.Success)
				{
					return Ok(result);
				}

				return BadRequest(result);
			}
			catch (Exception ex)
			{
				Logger.LogException(ExceptionCodes.Alert.GetUnreadCountByUser, nameof(GetUnreadAlertCountByUserId), ex, CurrentUserId);
				return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
			}
		}


		[HttpPut("mark-read")]
		public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
		{
			try
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
			catch (Exception ex)
			{
				Logger.LogException(ExceptionCodes.Alert.MarkAsRead, nameof(MarkAsRead), ex, CurrentUserId);
				return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
			}
		}



		[HttpPut("approve-request")]
		public async Task<IActionResult> ApproveRequestFromAlert([FromBody] ApproveRequestFromAlertRequest request)
		{
			try
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
			catch (Exception ex)
			{
				Logger.LogException(ExceptionCodes.Alert.ApproveRequestFromAlert, nameof(ApproveRequestFromAlert), ex, CurrentUserId);
				return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
			}
		}


		[HttpPut("reject-request")]
		public async Task<IActionResult> RejectRequestFromAlert([FromBody] RejectRequestFromAlertRequest request)
		{
			try
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

				var rejecterUserId = CurrentUserId ?? 0;
				Logger.LogInformation(LogMessages.Alert.RejectingRequestFromAlert, request.AlertId);

				var result = await _service.RejectRequestFromAlertAsync(request, rejecterUserId);
				if (result.Success)
				{
					return Ok(result);
				}
				return BadRequest(result);
			}
			catch (Exception ex)
			{
				Logger.LogException(ExceptionCodes.Alert.RejectRequestFromAlert, nameof(RejectRequestFromAlert), ex, CurrentUserId);
				return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
			}
		}

	}
}

