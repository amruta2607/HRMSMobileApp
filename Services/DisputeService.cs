using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Services
{
	public class DisputeService : IDisputeService
	{
		private readonly IDisputeRepository _repository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IAttendanceRepository _attendanceRepository;
		private readonly IApprovalWorkflowService _approvalWorkflowService;
		private readonly ILogger<DisputeService> _logger;

		public DisputeService(
			IDisputeRepository repository,
			IEmployeeRepository employeeRepository,
			IAttendanceRepository attendanceRepository,
			IApprovalWorkflowService approvalWorkflowService,
			ILogger<DisputeService> logger)
		{
			_repository = repository;
			_employeeRepository = employeeRepository;
			_attendanceRepository = attendanceRepository;
			_approvalWorkflowService = approvalWorkflowService;
			_logger = logger;
		}

		/// <summary>
		/// Resolve EmployeeId from UserId
		/// </summary>
		private async Task<int?> ResolveEmployeeIdFromUserIdAsync(int userId)
		{
			try
			{
				var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);

				if (employee == null)
				{
					_logger.LogWarning(LogMessages.EmployeeResolution.NoEmployeeFoundForUserId, userId);
					return null;
				}

				return employee.Id;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, LogMessages.EmployeeResolution.ErrorResolvingEmployeeIdFromUserId, userId);
				return null;
			}
		}

		public async Task<DisputeCategoryResponse> GetDisputeCategoriesAsync()
		{
			try
			{
				_logger.LogInformation(LogMessages.Dispute.FetchingDisputeCategories);

				var categories = await _repository.GetDisputeCategoriesAsync();

				var categoryList = categories
					.Where(c => c.IsActive)
					.Select(c => new DisputeCategoryDto
					{
						Id = c.Id,
						CategoryName = c.CategoryName,
						IsActive = c.IsActive
					})
					.ToList();

				return new DisputeCategoryResponse
				{
					Success = true,
					Message = DisputeMessages.DisputeCategoriesFetchedSuccessfully,
					Data = categoryList
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, LogMessages.Dispute.ErrorFetchingDisputeCategories);

				return new DisputeCategoryResponse
				{
					Success = false,
					Message = GeneralMessages.SomethingWentWrongContactAdmin,
					Data = null
				};
			}
		}

		/// <inheritdoc />
		public async Task<DisputeSubmitResponse> SubmitDisputeAsync(DisputeSubmitRequest request, int userId, int tenantId)
		{
			try
			{
				if (request.DisputeCategoryId <= 0)
				{
					return Fail(DisputeMessages.DisputeCategoryIdRequired);
				}

				if (string.IsNullOrWhiteSpace(request.Description))
				{
					return Fail(DisputeMessages.DescriptionRequired);
				}

				// SQL Server DateTime valid range
				DateTime minSqlDate = new DateTime(1753, 1, 1);
				DateTime maxSqlDate = new DateTime(9999, 12, 31);

				if (request.DisputeDate == default ||
					request.DisputeDate < minSqlDate ||
					request.DisputeDate > maxSqlDate)
				{
					return Fail(DisputeMessages.InvalidDisputeDate);
				}

				if (request.DisputeDate.Date > DateTime.Today)
				{
					return Fail(DisputeMessages.DisputeDateCannotBeFuture);
				}

				if (request.RequestedPunchInTime.HasValue &&
					request.RequestedPunchOutTime.HasValue &&
					request.RequestedPunchInTime.Value > request.RequestedPunchOutTime.Value)
				{
					return Fail(DisputeMessages.InvalidRequestedPunchTimes);
				}

				// Resolve EmployeeId from authenticated UserId (never accept from client)
				var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
				if (!employeeId.HasValue || employeeId.Value <= 0)
				{
					return Fail(DisputeMessages.EmployeeNotFoundForGivenUser);
				}

				_logger.LogInformation(LogMessages.Dispute.SubmittingDispute, employeeId.Value);

				var employee = await _repository.GetEmployeeByIdAsync(employeeId.Value);
				if (employee == null)
				{
					return Fail(DisputeMessages.EmployeeNotFound);
				}

				if (employee.OrganisationId != tenantId)
				{
					return Fail(DisputeMessages.EmployeeNotFound);
				}

				// Reporting manager = Employee.SupervisorId; required for approval routing
				if (employee.SupervisorId <= 0)
				{
					_logger.LogWarning(LogMessages.Dispute.NoReportingManager, employeeId.Value);
					return Fail(DisputeMessages.NoReportingManagerAssigned);
				}

				var manager = await _repository.GetEmployeeByIdAsync(employee.SupervisorId);
				if (manager == null || manager.SystemUserId <= 0)
				{
					_logger.LogWarning(LogMessages.Dispute.NoReportingManager, employeeId.Value);
					return Fail(DisputeMessages.ReportingManagerUserNotFound);
				}

				var managerUserId = manager.SystemUserId;

				// Optional integers default to 0 when null/not provided
				var punchId = request.PunchId;

				if (punchId > 0)
				{
					var punch = await _attendanceRepository.GetPunchByIdAsync(punchId, tenantId);
					if (punch == null || punch.EmployeeId != employeeId.Value)
					{
						return Fail(DisputeMessages.InvalidPunchId);
					}
				}

				// Web UX_EmployeeDispute_Unique: EmployeeId + DisputeCategoryId + DisputeDate
				var existingDispute = await _repository.GetExistingDisputeAsync(
					employeeId.Value,
					request.DisputeCategoryId,
					request.DisputeDate);
				if (existingDispute != null)
				{
					return Fail(DisputeMessages.OnlyOneDisputePerCategoryDate);
				}

				var dispute = new EmployeeDispute
				{
					EmployeeId = employeeId.Value,
					DisputeCategoryId = request.DisputeCategoryId,
					DisputeDate = request.DisputeDate.Date,
					Description = request.Description.Trim(),
					Status = "Pending",
					CreatedOn = DateTime.UtcNow,
					TenantId = tenantId,
					PunchId = punchId,
					RequestedPunchInTime = request.RequestedPunchInTime,
					RequestedPunchOutTime = request.RequestedPunchOutTime
				};

				var disputeId = await _repository.InsertDisputeAsync(dispute);
				if (disputeId <= 0)
				{
					return Fail(DisputeMessages.FailedToSubmitDispute);
				}

				dispute.Id = disputeId;

				_logger.LogInformation(LogMessages.Dispute.DisputeSubmittedSuccessfully, disputeId);
				_logger.LogInformation(
					LogMessages.Dispute.RoutingApprovalToManager,
					disputeId,
					managerUserId,
					manager.Id);

				// Initiate approval workflow assigned to reporting manager + Alert notifications
				try
				{
					var workflowResult = await _approvalWorkflowService.InitiateRegularizationRequestApprovalAsync(
						dispute,
						userId,
						tenantId,
						managerUserId);

					if (workflowResult.Success)
					{
						_logger.LogInformation(LogMessages.Dispute.ApprovalWorkflowInitiated, disputeId, workflowResult.EventId);
					}
					else
					{
						_logger.LogWarning(LogMessages.Dispute.ApprovalWorkflowNotConfigured, disputeId);
					}
				}
				catch (Exception ex)
				{
					_logger.LogWarning(ex, LogMessages.Dispute.ApprovalWorkflowNotConfigured, disputeId);
				}

				return new DisputeSubmitResponse
				{
					Success = true,
					Message = DisputeMessages.DisputeSubmittedSuccessfully,
					Data = new EmployeeDisputeDto
					{
						Id = dispute.Id,
						EmployeeId = dispute.EmployeeId,
						DisputeCategoryId = dispute.DisputeCategoryId,
						DisputeDate = dispute.DisputeDate,
						Description = dispute.Description,
						Status = dispute.Status,
						PunchId = dispute.PunchId,
						RequestedPunchInTime = dispute.RequestedPunchInTime,
						RequestedPunchOutTime = dispute.RequestedPunchOutTime,
						CreatedOn = dispute.CreatedOn
					}
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, LogMessages.Dispute.ErrorSubmittingDispute, userId);

				return new DisputeSubmitResponse
				{
					Success = false,
					Message = GeneralMessages.SomethingWentWrongContactAdmin,
					Data = null
				};
			}
		}

		/// <inheritdoc />
		public async Task<(bool Success, string Message)> ApproveDisputeAsync(int disputeId, int tenantId, int updateUserId)
		{
			try
			{
				_logger.LogInformation(LogMessages.Dispute.ApprovingDispute, disputeId, tenantId);
				return await _repository.ApproveDisputeAndApplyPunchCorrectionAsync(disputeId, tenantId, updateUserId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, LogMessages.Dispute.ErrorApprovingDispute, disputeId);
				return (false, DisputeMessages.FailedToApproveDispute);
			}
		}

		/// <inheritdoc />
		public async Task<(bool Success, string Message)> RejectDisputeAsync(int disputeId, int tenantId)
		{
			try
			{
				_logger.LogInformation(LogMessages.Dispute.RejectingDispute, disputeId, tenantId);
				return await _repository.RejectDisputeAsync(disputeId, tenantId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, LogMessages.Dispute.ErrorRejectingDispute, disputeId);
				return (false, DisputeMessages.FailedToRejectDispute);
			}
		}

		private static DisputeSubmitResponse Fail(string message) =>
			new()
			{
				Success = false,
				Message = message,
				Data = null
			};
	}
}
