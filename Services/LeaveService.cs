using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using Microsoft.Extensions.Logging;

namespace MobileWebApi.Services
{
	public class LeaveService : ILeaveService
	{
		private readonly ILeaveRepository _leaveRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IApprovalWorkflowService _approvalWorkflowService;
		private readonly ILogger<LeaveService> _logger;

		// -----------------------------
		// Status strings
		// -----------------------------
		private const string STATUS_SUBMIT = "Submit";
		private const string STATUS_APPROVED = "Approved";
		private const string STATUS_REJECTED = "Rejected";
		private const string STATUS_CANCELLED = "Cancelled";
		private const string STATUS_WITHDRAW = "Withdrawn";
		private const string STATUS_PENDING = "Pending";


		// -----------------------------
		// Status IDs in DB
		// -----------------------------
		private const int STATUS_ID_SUBMIT = 1;
		private const int STATUS_ID_APPROVED = 2;
		private const int STATUS_ID_REJECTED = 3;
		private const int STATUS_ID_CANCELLED = 5;
		private const int STATUS_ID_WITHDRAW = 4;
		private const int STATUS_ID_PENDING = 6;

		public LeaveService(
			ILeaveRepository leaveRepository,
			IEmployeeRepository employeeRepository,
			IApprovalWorkflowService approvalWorkflowService,
			ILogger<LeaveService> logger)
		{
			_leaveRepository = leaveRepository;
			_employeeRepository = employeeRepository;
			_approvalWorkflowService = approvalWorkflowService;
			_logger = logger;
		}

		// =====================================================
		// CREATE LEAVE REQUEST
		// =====================================================
		public async Task<LeaveRequestResponse> CreateLeaveRequestAsync(LeaveRequestCreateRequest request)
		{
			try
			{
				if (request.user <= 0 || request.leave_type <= 0)
					return Fail(LeaveMessages.InvalidRequest);

				var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(request.user);
				if (!employeeId.HasValue)
					return Fail(LeaveMessages.EmployeeNotFoundForUser);

				var leaveBalance = await _leaveRepository.GetLeaveBalanceAsync(employeeId.Value, request.leave_type);
				decimal availableBalance = leaveBalance?.RemainingBalance ?? 0;
				decimal duration = request.is_half_day ? 0.5m : request.duration;

				if (availableBalance < duration)
					return Fail(string.Format(LeaveMessages.InsufficientLeaveBalance, availableBalance, duration));

				var requestNumber = await _leaveRepository.GenerateLeaveRequestNumberAsync(request.organization ?? 0);

				var leaveRequest = new LeaveRequest
				{
					Number = requestNumber,
					EmployeeId = employeeId.Value,
					LeaveTypeId = request.leave_type,
					LeaveBalance = availableBalance,
					FromDate = request.startdate,
					ToDate = request.enddate,
					Duration = duration,
					Description = request.reason,
					CurrentAction = STATUS_SUBMIT,
					LeaveRequestStatus = STATUS_ID_SUBMIT,
					OrganisationId = request.organization,
					InsertUserId = request.user,
					InsertDate = DateTime.Now
				};

				var newId = await _leaveRepository.CreateLeaveRequestAsync(leaveRequest);
				if (newId <= 0)
					return Fail(LeaveMessages.FailedToCreateLeaveRequest);

				leaveRequest.Id = newId;

				try
				{
					if (request.organization.HasValue)
					{
						await _approvalWorkflowService.InitiateLeaveRequestApprovalAsync(
							leaveRequest,
							request.user,
							request.organization.Value);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, "Approval workflow not configured");
				}

				return Success(LeaveMessages.LeaveRequestSubmittedSuccessfully, new { Id = newId, Number = requestNumber });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error creating leave request");
				return Fail(ex.Message);
			}
		}

		// =====================================================
		// GET LEAVE REQUESTS
		// =====================================================
		public async Task<LeaveRequestResponse> GetLeaveRequestsAsync(LeaveRequestGetRequest request)
		{
			try
			{
				int? employeeId = null;

				if (request.user.HasValue)
					employeeId = await _leaveRepository
						.GetEmployeeIdByUserIdAsync(request.user.Value);

				var list = (await _leaveRepository.GetLeaveRequestsAsync(
					request.organization,
					employeeId,
					request.leave_type
				)).ToList();

				// ✅ Convert status ID → status text
				foreach (var item in list)
				{
					item.LeaveRequestStatusText =
						MapDbStatusIdToText((int)item.LeaveRequestStatus);
				}

				return new LeaveRequestResponse
				{
					Success = true,
					Message = LeaveMessages.LeaveRequestsFetchedSuccessfully,
					Data = list,
					TotalRecords = list.Count
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "GetLeaveRequestsAsync failed");
				return Fail(ex.Message);
			}
		}
		private string MapDbStatusIdToText(int statusId)
		{
			return statusId switch
			{
				STATUS_ID_SUBMIT => STATUS_SUBMIT,
				STATUS_ID_APPROVED => STATUS_APPROVED,
				STATUS_ID_REJECTED => STATUS_REJECTED,
				STATUS_ID_CANCELLED => STATUS_CANCELLED,
				STATUS_ID_WITHDRAW => STATUS_WITHDRAW,
				STATUS_ID_PENDING => STATUS_PENDING,
				_ => "Unknown"
			};
		}



		// =====================================================
		// GET LEAVE REQUEST BY ID
		// =====================================================
		public async Task<LeaveRequestResponse> GetLeaveRequestByIdAsync(int id)
		{
			var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);
			if (leave == null)
				return Fail(LeaveMessages.LeaveRequestNotFound);

			return Success(LeaveMessages.LeaveRequestFetchedSuccessfully, leave);
		}

		// =====================================================
		// GET LEAVE BALANCE
		// =====================================================
		public async Task<LeaveBalanceResponse> GetLeaveBalanceAsync(int userId, int? organization)
		{
			var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(userId);
			if (!employeeId.HasValue)
				return new LeaveBalanceResponse
				{
					Success = false,
					Message = LeaveMessages.EmployeeNotFoundForUser
				};

			var balances = await _leaveRepository.GetLeaveBalanceByEmployeeIdAsync(employeeId.Value);

			var responseData = balances.Select(b => new LeaveBalanceDetail
			{
				LeaveTypeId = b.LeaveTypeId,
				LeaveTypeName = b.LeaveTypeName,
				TotalBalance = b.TotalBalance,
			
				RemainingBalance = b.RemainingBalance // Total - Used
			}).ToList();

			return new LeaveBalanceResponse
			{
				Success = true,
				Message = LeaveMessages.LeaveBalanceFetchedSuccessfully,
				Data = responseData
			};
		}

		// =====================================================
		// STATUS ACTIONS
		// =====================================================
		public Task<LeaveRequestResponse> ApproveLeaveRequestAsync(int id, int approverUserId)
			=> UpdateStatus(id, STATUS_ID_APPROVED, STATUS_APPROVED, approverUserId);

		public Task<LeaveRequestResponse> RejectLeaveRequestAsync(int id, int userId, string? reason)
			=> UpdateStatus(id, STATUS_ID_REJECTED, STATUS_REJECTED, userId);

		public Task<LeaveRequestResponse> CancelLeaveRequestAsync(int id, int userId, string? reason)
			=> UpdateStatus(id, STATUS_ID_CANCELLED, STATUS_CANCELLED, userId);

		public async Task<LeaveRequestResponse> WithdrawLeaveRequestAsync(int id, int userId, string? reason)
		{
			var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);
			if (leave == null)
				return Fail(LeaveMessages.LeaveRequestNotFound);

			if (leave.LeaveRequestStatus != STATUS_ID_SUBMIT)
				return Fail("Only pending leave requests can be withdrawn");

			await _leaveRepository.UpdateLeaveRequestStatusAsync(
				id,
				STATUS_ID_WITHDRAW,
				STATUS_WITHDRAW,
				userId);

			return Success("Leave request withdrawn successfully", new { Id = id });
		}

		// =====================================================
		// HELPERS
		// =====================================================
		private async Task<LeaveRequestResponse> UpdateStatus(int id, int statusId, string statusText, int userId)
		{
			var updated = await _leaveRepository.UpdateLeaveRequestStatusAsync(id, statusId, statusText, userId);
			return updated ? Success("Status updated successfully", new { Id = id }) : Fail("Failed to update status");
		}

		private int? MapMobileStatusToDbStatus(string? status)
		{
			if (string.IsNullOrWhiteSpace(status)) return null;

			return status.ToLower() switch
			{
				"submit" => STATUS_ID_SUBMIT,
				"approved" => STATUS_ID_APPROVED,
				"rejected" => STATUS_ID_REJECTED,
				"cancelled" => STATUS_ID_CANCELLED,
				"withdraw" => STATUS_ID_WITHDRAW,
				"pending" => STATUS_ID_PENDING,

				_ => null
			};
		}

		private LeaveRequestResponse Fail(string message) =>
			new LeaveRequestResponse { Success = false, Message = message };

		private LeaveRequestResponse Success(string message, object? data = null) =>
			new LeaveRequestResponse { Success = true, Message = message, Data = data, TotalRecords = 1 };
	}
}
