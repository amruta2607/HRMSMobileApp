using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly IDisputeRepository _repository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<DisputeService> _logger;

        public DisputeService(
            IDisputeRepository repository,
            IEmployeeRepository employeeRepository,
            ILogger<DisputeService> logger)
        {
            _repository = repository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        /// <summary>
        /// Resolves EmployeeId from UserId by joining Users and Employee tables
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
                    Message = "Dispute categories fetched successfully",
                    Data = categoryList
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Dispute.ErrorFetchingDisputeCategories);
                return new DisputeCategoryResponse
                {
                    Success = false,
                    Message = $"Error fetching dispute categories: {ex.Message}",
                    Data = null
                };
            }
        }

        public async Task<DisputeSubmitResponse> SubmitDisputeAsync(DisputeSubmitRequest request)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(request.UserId);
                if (!employeeId.HasValue)
                {
                    return new DisputeSubmitResponse
                    {
                        Success = false,
                        Message = "Employee not found for the given user",
                        Data = null
                    };
                }

                _logger.LogInformation(LogMessages.Dispute.SubmittingDispute, employeeId.Value);

                // Validate DisputeDate is not a future date
                if (request.DisputeDate.Date > DateTime.Today)
                {
                    return new DisputeSubmitResponse
                    {
                        Success = false,
                        Message = "Dispute date cannot be a future date",
                        Data = null
                    };
                }

                // Validate Description is mandatory
                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    return new DisputeSubmitResponse
                    {
                        Success = false,
                        Message = "Description is required",
                        Data = null
                    };
                }

                // Validate EmployeeId exists
                var employee = await _repository.GetEmployeeByIdAsync(employeeId.Value);
                if (employee == null)
                {
                    return new DisputeSubmitResponse
                    {
                        Success = false,
                        Message = "Employee not found",
                        Data = null
                    };
                }

                // Validate that employee can submit only one dispute per day
                var existingDispute = await _repository.GetExistingDisputeAsync(
                    employeeId.Value, 
                    request.DisputeDate);

                if (existingDispute != null)
                {
                    return new DisputeSubmitResponse
                    {
                        Success = false,
                        Message = "Only one dispute can be submitted per day. A dispute for this date already exists",
                        Data = null
                    };
                }

                // Create dispute entity
                var dispute = new EmployeeDispute
                {
                    EmployeeId = employeeId.Value,
                    DisputeCategoryId = request.DisputeCategoryId,
                    DisputeDate = request.DisputeDate.Date,
                    Description = request.Description.Trim(),
                    Status = "Pending",
                    CreatedOn = DateTime.Now
                };

                // Insert dispute
                var disputeId = await _repository.InsertDisputeAsync(dispute);
                
                if (disputeId > 0)
                {
                    dispute.Id = disputeId;
                    
                    var disputeDto = new EmployeeDisputeDto
                    {
                        Id = dispute.Id,
                        DisputeCategoryId = dispute.DisputeCategoryId,
                        DisputeDate = dispute.DisputeDate,
                        Description = dispute.Description,
                        Status = dispute.Status,
                        CreatedOn = dispute.CreatedOn
                    };

                    _logger.LogInformation(LogMessages.Dispute.DisputeSubmittedSuccessfully, disputeId);
                    
                    return new DisputeSubmitResponse
                    {
                        Success = true,
                        Message = "Dispute submitted successfully",
                        Data = disputeDto
                    };
                }

                return new DisputeSubmitResponse
                {
                    Success = false,
                    Message = "Failed to submit dispute",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Dispute.ErrorSubmittingDispute, request.UserId);
                return new DisputeSubmitResponse
                {
                    Success = false,
                    Message = $"Error submitting dispute: {ex.Message}",
                    Data = null
                };
            }
        }
    }
}

