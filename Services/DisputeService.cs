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
		private readonly ILogger<DisputeService> _logger;

		public DisputeService(
			IDisputeRepository repository,
			IEmployeeRepository employeeRepository,
			IAttendanceRepository attendanceRepository,
			ILogger<DisputeService> logger)
		{
			_repository = repository;
			_employeeRepository = employeeRepository;
			_attendanceRepository = attendanceRepository;
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
		public async Task<DisputeSubmitResponse> SubmitDisputeAsync(DisputeSubmitRequest request, int tenantId)
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

				// Resolve EmployeeId from UserId (authoritative for the authenticated user)
				var resolvedEmployeeId = await ResolveEmployeeIdFromUserIdAsync(request.UserId);
				if (!resolvedEmployeeId.HasValue)
				{
					return Fail(DisputeMessages.EmployeeNotFoundForGivenUser);
				}

				// Prefer request.EmployeeId when provided; otherwise use resolved value (backward compatible)
				var employeeId = request.EmployeeId > 0 ? request.EmployeeId : resolvedEmployeeId.Value;
				if (employeeId <= 0)
				{
					return Fail(DisputeMessages.EmployeeIdRequired);
				}

				// Non-elevated callers cannot submit for a different employee
				if (request.EmployeeId > 0 && request.EmployeeId != resolvedEmployeeId.Value)
				{
					return Fail(DisputeMessages.EmployeeNotFound);
				}

				_logger.LogInformation(LogMessages.Dispute.SubmittingDispute, employeeId);

				var employee = await _repository.GetEmployeeByIdAsync(employeeId);
				if (employee == null)
				{
					return Fail(DisputeMessages.EmployeeNotFound);
				}

				if (employee.OrganisationId != tenantId)
				{
					return Fail(DisputeMessages.EmployeeNotFound);
				}

				if (request.PunchId.HasValue && request.PunchId.Value > 0)
				{
					var punch = await _attendanceRepository.GetPunchByIdAsync(request.PunchId.Value, tenantId);
					if (punch == null || punch.EmployeeId != employeeId)
					{
						return Fail(DisputeMessages.InvalidPunchId);
					}
				}

				var existingDispute = await _repository.GetExistingDisputeAsync(employeeId, request.DisputeDate);
				if (existingDispute != null)
				{
					return Fail(DisputeMessages.OnlyOneDisputePerDay);
				}

				var dispute = new EmployeeDispute
				{
					EmployeeId = employeeId,
					DisputeCategoryId = request.DisputeCategoryId,
					DisputeDate = request.DisputeDate.Date,
					Description = request.Description.Trim(),
					Status = "Pending",
					CreatedOn = DateTime.UtcNow,
					TenantId = tenantId,
					PunchId = request.PunchId > 0 ? request.PunchId : null,
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
				_logger.LogError(ex, LogMessages.Dispute.ErrorSubmittingDispute, request.UserId);

				return new DisputeSubmitResponse
				{
					Success = false,
					Message = GeneralMessages.SomethingWentWrongContactAdmin,
					Data = null
				};
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
