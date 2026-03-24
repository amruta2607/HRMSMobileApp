using MobileWebApi.Constants;
using MobileWebApi.Helper;
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
		// -----------------------------
		// Status strings
		// -----------------------------
		private const string STATUS_SUBMIT = "Submit";
		private const string STATUS_APPROVED = "Approved";
		private const string STATUS_REJECTED = "Rejected";
		private const string STATUS_WITHDRAW = "Withdrawn";
		private const string STATUS_CANCELLED = "Canceled";
		private const string STATUS_PENDING = "Pending";
		private const string STATUS_PENDING_FOR_APPROVAL = "Pending For Approval";
		private const string STATUS_CANCELLATION_APPROVED = "Cancellation Approved";
		private const string STATUS_CANCELLATION_REJECTED = "Cancellation Rejected";


		// -----------------------------
		// Status IDs in DB
		// -----------------------------
		// -----------------------------
		// Status IDs in DB
		// -----------------------------
		private const int STATUS_ID_SUBMIT = 1;
		private const int STATUS_ID_APPROVED = 2;
		private const int STATUS_ID_REJECTED = 3;
		private const int STATUS_ID_WITHDRAW = 4;
		private const int STATUS_ID_CANCELLED = 5;
		private const int STATUS_ID_PENDING = 6;
		private const int STATUS_ID_PENDING_FOR_APPROVAL = 7;
		private const int STATUS_ID_CANCELLATION_APPROVED = 8;
		private const int STATUS_ID_CANCELLATION_REJECTED = 9;

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
				// -----------------------------
				// Validate basic input
				// -----------------------------
				if (request.user <= 0 || request.leave_type <= 0)
					return Fail(LeaveMessages.InvalidRequest);

				var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(request.user);
				if (!employeeId.HasValue)
					return Fail(LeaveMessages.EmployeeNotFoundForUser);

				// -----------------------------
				// Get configured week offs & holidays
				// -----------------------------
				var fromDate = request.startdate.Date;
				var toDate = request.enddate.Date;

				var dayOffs = await _leaveRepository.GetTenantDayOffsAsync(request.organization ?? 0); // returns List<int> for DayOffId (1=Sunday etc.)
				var holidays = await _leaveRepository.GetHolidaysAsync(request.organization ?? 0, fromDate, toDate);

				// -----------------------------
				// Determine valid leave dates
				// -----------------------------
				var requestedDates = EachDate(fromDate, toDate).ToList();

				var invalidDates = requestedDates
					.Where(d => dayOffs.Contains((int)d.DayOfWeek) || holidays.Any(h => h.Date.Date == d.Date))
					.ToList();

				if (invalidDates.Any())
				{
					var invalidDatesStr = string.Join(", ", invalidDates.Select(d => d.ToString("yyyy-MM-dd")));
					return Fail($"Cannot apply leave on week offs or holidays: {invalidDatesStr}");
				}
				// -----------------------------
				// Prevent duplicate / overlapping leave
				// -----------------------------
				var hasOverlap = await _leaveRepository.HasOverlappingLeaveAsync(
					employeeId.Value,
					fromDate,
					toDate
				);

				if (hasOverlap)
				{
					return Fail(LeaveMessages.LeaveAlreadyAppliedForSelectedDate);
				}

				// -----------------------------
				// Calculate leave balance & duration
				// -----------------------------
				var leaveBalance = await _leaveRepository.GetLeaveBalanceAsync(employeeId.Value, request.leave_type);
				decimal availableBalance = leaveBalance?.RemainingBalance ?? 0;

				decimal duration = request.is_half_day ? 0.5m : requestedDates.Count;

				if (availableBalance < duration)
					return Fail(string.Format(LeaveMessages.InsufficientLeaveBalance, availableBalance, duration));

				// -----------------------------
				// Generate leave request number
				// -----------------------------
				var requestNumber = await GenerateLeaveRequestNumberAsync(request.organization ?? 0);
				// -----------------------------
				// Create leave request
				// -----------------------------
				var leaveRequest = new LeaveRequest
				{
					Number = requestNumber,
					EmployeeId = employeeId.Value,
					LeaveTypeId = request.leave_type,
					LeaveBalance = availableBalance,
					FromDate = fromDate,
					ToDate = toDate,
					Duration = duration,
					Description = request.reason,
					CurrentAction = STATUS_SUBMIT,
					LeaveRequestStatus = STATUS_ID_PENDING,
					OrganisationId = request.organization,
					InsertUserId = request.user,
					InsertDate = DateTime.Now
				};

				var newId = await _leaveRepository.CreateLeaveRequestAsync(leaveRequest);
				if (newId <= 0)
					return Fail(LeaveMessages.FailedToCreateLeaveRequest);

				leaveRequest.Id = newId;

				// -----------------------------
				// Initiate approval workflow if configured
				// -----------------------------
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
					_logger.LogWarning(ex, LogMessages.ApprovalWorkflow.ApprovalWorkflowNotConfigured);
				}

				return Success(LeaveMessages.LeaveRequestSubmittedSuccessfully, new { Id = newId, Number = requestNumber });
			}
			catch (Exception ex)
			{
				_logger.LogException(ExceptionCodes.Leave.CreateLeaveRequest, nameof(CreateLeaveRequestAsync), ex, request.user);
				return Fail(string.Format(GeneralMessages.SomethingWentWrongWithCode, ExceptionCodes.Leave.CreateLeaveRequest));
			}
		}

		// -----------------------------
		// Helper: iterate through all dates
		// -----------------------------
		private IEnumerable<DateTime> EachDate(DateTime from, DateTime to)
		{
			for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
				yield return day;
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
				_logger.LogException(ExceptionCodes.Leave.GetLeaveRequests, nameof(GetLeaveRequestsAsync), ex, request.user);
				return Fail(string.Format(GeneralMessages.SomethingWentWrongWithCode, ExceptionCodes.Leave.GetLeaveRequests));
			}
		}
		private string MapDbStatusIdToText(int statusId)
		{
			return statusId switch
			{
				STATUS_ID_SUBMIT => STATUS_SUBMIT,
				STATUS_ID_APPROVED => STATUS_APPROVED,
				STATUS_ID_REJECTED => STATUS_REJECTED,
				STATUS_ID_WITHDRAW => STATUS_WITHDRAW,
				STATUS_ID_CANCELLED => STATUS_CANCELLED,
				STATUS_ID_PENDING => STATUS_PENDING,
				STATUS_ID_PENDING_FOR_APPROVAL => STATUS_PENDING_FOR_APPROVAL,
				STATUS_ID_CANCELLATION_APPROVED => STATUS_CANCELLATION_APPROVED,
				STATUS_ID_CANCELLATION_REJECTED => STATUS_CANCELLATION_REJECTED,
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

			if (leave.LeaveRequestStatus != STATUS_ID_SUBMIT && leave.LeaveRequestStatus != STATUS_ID_PENDING)
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

		// =====================================================
		// GET LEAVE HISTORY
		// =====================================================
		public async Task<LeaveHistoryResponse> GetLeaveHistoryAsync(int userId)
		{
			try
			{
				var targetYear = DateTime.Now.Year;

				var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(userId);
				if (!employeeId.HasValue)
					return new LeaveHistoryResponse
					{
						Success = false,
						Message = LeaveMessages.EmployeeNotFoundForUser
					};

				var history = (await _leaveRepository.GetLeaveHistoryAsync(employeeId.Value, targetYear)).ToList();

				var leavesAvailed = history.Count(h => h.Status == STATUS_APPROVED);

				return new LeaveHistoryResponse
				{
					Success = true,
					Message = LeaveMessages.LeaveHistoryFetchedSuccessfully,
					LeavesAvailed = leavesAvailed,
					Year = targetYear,
					Data = history
				};
			}
			catch (Exception ex)
			{
				_logger.LogException(ExceptionCodes.Leave.GetLeaveHistory, nameof(GetLeaveHistoryAsync), ex, userId);
				return new LeaveHistoryResponse
				{
					Success = false,
					Message = string.Format(GeneralMessages.SomethingWentWrongWithCode, ExceptionCodes.Leave.GetLeaveHistory)
				};
			}
		}

		private LeaveRequestResponse Fail(string message) =>
			new LeaveRequestResponse { Success = false, Message = message };

		private LeaveRequestResponse Success(string message, object? data = null) =>
			new LeaveRequestResponse { Success = true, Message = message, Data = data, TotalRecords = 1 };
		private async Task<string> GenerateLeaveRequestNumberAsync(int organisationId)
		{
			var today = DateTime.Now.ToString("yyyyMMdd");

			var lastNumber = await _leaveRepository
				.GetLastLeaveRequestNumberAsync(today, organisationId);

			int nextSequence = 1;

			if (!string.IsNullOrEmpty(lastNumber))
			{
				var seqPart = lastNumber.Substring(lastNumber.Length - 4);

				if (int.TryParse(seqPart, out int seq))
				{
					nextSequence = seq + 1;
				}
			}

			return $"LVR/{today}{nextSequence:D4}";
		}
	}
}
