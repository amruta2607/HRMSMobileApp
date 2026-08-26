using System.Collections.Generic;
using System.Globalization;
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
		private readonly IApprovalRepository _approvalRepository;
		private readonly ILogger<LeaveService> _logger;

		// -----------------------------
		// Status strings
		// -----------------------------
		private const string STATUS_SUBMIT = "Submit";
		private const string STATUS_APPROVED = "Approved";
		private const string STATUS_REJECTED = "Rejected";
		private const string STATUS_CANCELLED = "Cancelled";
		private const string STATUS_WITHDRAW = "Withdraw";
		private const string STATUS_PENDING = "Pending";
		private const string STATUS_PENDING_FOR_APPROVAL = "Pending For Approval";
		private const string STATUS_CANCELLATION_APPROVED = "Cancellation Approved";
		private const string STATUS_CANCELLATION_REJECTED = "Cancellation Rejected";


		// -----------------------------
		// Status IDs in DB
		// -----------------------------
		private const int STATUS_ID_SUBMIT = 1;
		private const int STATUS_ID_APPROVED = 2;
		private const int STATUS_ID_REJECTED = 3;
		private const int STATUS_ID_CANCELLED = 5;
		private const int STATUS_ID_WITHDRAW = 4;
		private const int STATUS_ID_PENDING = 6;
		private const int STATUS_ID_PENDING_FOR_APPROVAL = 7;
		private const int STATUS_ID_CANCELLATION_APPROVED = 8;
		private const int STATUS_ID_CANCELLATION_REJECTED = 9;

		public LeaveService(
			ILeaveRepository leaveRepository,
			IEmployeeRepository employeeRepository,
			IApprovalWorkflowService approvalWorkflowService,
			IApprovalRepository approvalRepository,
			ILogger<LeaveService> logger)
		{
			_leaveRepository = leaveRepository;
			_employeeRepository = employeeRepository;
			_approvalWorkflowService = approvalWorkflowService;
			_approvalRepository = approvalRepository;
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
				var dayOffs = await _leaveRepository.GetTenantDayOffsAsync(request.organization ?? 0); // returns List<int> for DayOffId (1=Sunday etc.)
				var holidays = await _leaveRepository.GetHolidaysAsync(request.organization ?? 0, request.startdate, request.enddate);

				// -----------------------------
				// Determine valid leave dates
				// -----------------------------
				var requestedDates = EachDate(request.startdate, request.enddate).ToList();

				var invalidDates = requestedDates
					.Where(d => dayOffs.Contains((int)d.DayOfWeek) || holidays.Any(h => h.Date.Date == d.Date))
					.ToList();

				if (invalidDates.Any())
				{
					var invalidDatesStr = string.Join(", ", invalidDates.Select(d => d.ToString("yyyy-MM-dd")));
					return Fail($"Cannot apply leave on week offs or holidays: {invalidDatesStr}");
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
				var requestNumber = await _leaveRepository.GenerateLeaveRequestNumberAsync(request.organization ?? 0);

				// -----------------------------
				// Create leave request
				// -----------------------------
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
					InsertDate = DateTime.Now,
					HalfDayType=request.HalfDayType
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

		// -----------------------------
		// Helper: iterate through all dates
		// -----------------------------
		private IEnumerable<DateTime> EachDate(DateTime from, DateTime to)
		{
			for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
				yield return day;
		}

		public static int CalculateLeaveDays(DateTime fromDate, DateTime toDate, List<DateTime> holidays)
		{
			if (toDate.Date < fromDate.Date)
				return 0;

			var holidaySet = new HashSet<DateTime>(holidays.Select(h => h.Date));
			var count = 0;

			for (var day = fromDate.Date; day <= toDate.Date; day = day.AddDays(1))
			{
				if (holidaySet.Contains(day))
					continue;

				count++;
			}

			return count;
		}

		private static string FormatLeaveDates(DateTime fromDate, DateTime toDate)
		{
			const string fmt = "dd-MM-yyyy";
			var fromStr = fromDate.ToString(fmt, CultureInfo.InvariantCulture);
			var toStr = toDate.ToString(fmt, CultureInfo.InvariantCulture);
			return fromDate.Date == toDate.Date ? fromStr : $"{fromStr} - {toStr}";
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
						MapDbStatusIdToText(item.LeaveRequestStatus ?? 0);
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

			// Enforce: user can only withdraw their own leave request
			var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(userId);
			if (!employeeId.HasValue || leave.EmployeeId != employeeId.Value)
				return Fail(TenantAccessMessages.UserAccessDeniedSimple);

			if (leave.LeaveRequestStatus != STATUS_ID_SUBMIT && leave.LeaveRequestStatus != STATUS_ID_PENDING)
				return Fail("Only pending leave requests can be withdrawn");

			await _leaveRepository.UpdateLeaveRequestStatusAsync(
				id,
				STATUS_ID_WITHDRAW,
				STATUS_WITHDRAW,
				userId);

			// Mark existing screen notifications related to this leave request as read
			if (leave.OrganisationId.HasValue)
			{
				await _approvalRepository.MarkScreenNotificationsReadByLeaveRequestIdAsync(
					id,
					leave.OrganisationId.Value,
					userId);
			}

			return Success("Leave request withdrawn successfully", new { Id = id });
		}

		/// <summary>
		/// Get leave history for the logged-in user (current year).
		/// </summary>
		public async Task<LeaveHistoryResponse> GetLeaveHistoryAsync(int userId)
		{
			try
			{
				if (userId <= 0)
				{
					return new LeaveHistoryResponse
					{
						Success = false,
						Message = LeaveMessages.UserIdRequired,
						Year = DateTime.Now.Year,
						LeavesAvailed = 0,
						Data = null
					};
				}

				var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(userId);
				if (!employeeId.HasValue)
				{
					return new LeaveHistoryResponse
					{
						Success = false,
						Message = LeaveMessages.EmployeeNotFoundForUser,
						Year = DateTime.Now.Year,
						LeavesAvailed = 0,
						Data = null
					};
				}

				var year = DateTime.Now.Year;
				var leaveRequests = await _leaveRepository.GetLeaveRequestsByEmployeeIdAsync(employeeId.Value);

				var history = new List<LeaveHistoryItem>();

				foreach (var lr in leaveRequests)
				{
					foreach (var day in EachDate(lr.FromDate, lr.ToDate))
					{
						if (day.Year != year)
							continue;

						history.Add(new LeaveHistoryItem
						{
							LeaveDate = day.Date,
							LeaveType = lr.LeaveTypeName,
							Reason = lr.Description,
							Status = MapDbStatusIdToText(lr.LeaveRequestStatus ?? 0)
						});
					}
				}

				history = history.OrderBy(h => h.LeaveDate).ToList();

				return new LeaveHistoryResponse
				{
					Success = true,
					Message = LeaveMessages.LeaveHistoryFetchedSuccessfully,
					LeavesAvailed = history.Count,
					Year = year,
					Data = history
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "GetLeaveHistoryAsync failed for user {UserId}", userId);
				return new LeaveHistoryResponse
				{
					Success = false,
					Message = GeneralMessages.SomethingWentWrongContactAdmin,
					LeavesAvailed = 0,
					Year = DateTime.Now.Year,
					Data = null
				};
			}
		}

		public async Task<LeaveHistorySummaryResponse> GetLeaveHistorySummaryAsync(int userId)
		{
			try
			{
				if (userId <= 0)
				{
					return new LeaveHistorySummaryResponse
					{
						Success = false,
						Message = LeaveMessages.UserIdRequired,
						EmployeeId = 0,
						AvailableLeaves = 0,
						Year = DateTime.Now.Year,
						LeaveHistory = null
					};
				}

				var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(userId);
				if (!employeeId.HasValue)
				{
					return new LeaveHistorySummaryResponse
					{
						Success = false,
						Message = LeaveMessages.EmployeeNotFoundForUser,
						EmployeeId = 0,
						AvailableLeaves = 0,
						Year = DateTime.Now.Year,
						LeaveHistory = null
					};
				}

				var year = DateTime.Now.Year;
				var leaveRequests = (await _leaveRepository.GetLeaveRequestsByEmployeeIdAsync(employeeId.Value))
					.Where(lr => lr.FromDate.Year == year)
					.ToList();

				// Pre-fetch holidays once per tenant across the min/max leave range.
				var holidaysByTenant = new Dictionary<int, List<DateTime>>();
				var tenantGroups = leaveRequests
					.Where(lr => lr.OrganisationId.HasValue)
					.GroupBy(lr => lr.OrganisationId!.Value)
					.ToList();

				foreach (var grp in tenantGroups)
				{
					var minFrom = grp.Min(lr => lr.FromDate).Date;
					var maxTo = grp.Max(lr => lr.ToDate).Date;
					var hols = await _leaveRepository.GetHolidaysAsync(grp.Key, minFrom, maxTo);
					holidaysByTenant[grp.Key] = hols.Select(h => h.Date.Date).Distinct().ToList();
				}

				var items = new List<LeaveHistorySummaryItem>();
				decimal usedLeaves = 0m;

				foreach (var lr in leaveRequests.OrderByDescending(x => x.InsertDate ?? DateTime.MinValue).ThenByDescending(x => x.Id))
				{
					var status = MapDbStatusIdToText(lr.LeaveRequestStatus ?? 0);

					// Leave history should include all statuses
					items.Add(new LeaveHistorySummaryItem
					{
						LeaveRequestId = lr.Id,
						LeaveDates = FormatLeaveDates(lr.FromDate, lr.ToDate),
						LeaveType = lr.LeaveTypeName,
						Reason = lr.Description,
						Duration = lr.Duration,
						Status = status
					});

					// UsedLeaves should count only Approved leave requests
					if (string.Equals(status, STATUS_APPROVED, StringComparison.OrdinalIgnoreCase))
						usedLeaves += lr.Duration;
					
				}

				// EmployeeLeave.LeaveBalance is treated as the current available balance.
				// So AvailableLeaves should come directly from SUM(EmployeeLeave.LeaveBalance).
				var availableLeaves = await _leaveRepository.GetTotalLeaveAllocationForEmployeeAsync(employeeId.Value);

				return new LeaveHistorySummaryResponse
				{
					Success = true,
					Message = LeaveMessages.LeaveHistoryFetchedSuccessfully,
					EmployeeId = employeeId.Value,
					AvailableLeaves = availableLeaves,
					UsedLeaves = usedLeaves,
					Year = year,
					LeaveHistory = items
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "GetLeaveHistorySummaryAsync failed for user {UserId}", userId);
				return new LeaveHistorySummaryResponse
				{
					Success = false,
					Message = GeneralMessages.SomethingWentWrongContactAdmin,
					EmployeeId = 0,
					AvailableLeaves = 0,
					Year = DateTime.Now.Year,
					LeaveHistory = null
				};
			}
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
				"pending for approval" => STATUS_ID_PENDING_FOR_APPROVAL,
				"pendingforapproval" => STATUS_ID_PENDING_FOR_APPROVAL,
				"cancellation approved" => STATUS_ID_CANCELLATION_APPROVED,
				"cancellationapproved" => STATUS_ID_CANCELLATION_APPROVED,
				"cancellation rejected" => STATUS_ID_CANCELLATION_REJECTED,
				"cancellationrejected" => STATUS_ID_CANCELLATION_REJECTED,

				_ => null
			};
		}

		private LeaveRequestResponse Fail(string message) =>
			new LeaveRequestResponse { Success = false, Message = message };

		private LeaveRequestResponse Success(string message, object? data = null) =>
			new LeaveRequestResponse { Success = true, Message = message, Data = data, TotalRecords = 1 };
	}
}
