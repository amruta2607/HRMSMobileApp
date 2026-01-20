using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly ILeaveRepository _leaveRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IApprovalWorkflowService _approvalWorkflowService;
        private readonly ILogger<LeaveService> _logger;

        // Leave request statuses (string for CurrentAction column)
        private const string STATUS_SUBMIT = "Submit";
        private const string STATUS_PENDING = "Pending";
        private const string STATUS_APPROVED = "Approved";
        private const string STATUS_REJECTED = "Rejected";
        private const string STATUS_CANCELLED = "Cancelled";

        // Leave request status IDs (int for LeaveRequestStatus column)
        private const int STATUS_ID_SUBMIT = 1;
        private const int STATUS_ID_APPROVED = 2;
        private const int STATUS_ID_REJECTED = 3;
        private const int STATUS_ID_CANCELLED = 4;

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

        /// <summary>
        /// Create a new leave request from mobile app
        /// </summary>
        public async Task<LeaveRequestResponse> CreateLeaveRequestAsync(LeaveRequestCreateRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Leave.CreatingLeaveRequest, request.user);
                
                // Validate required fields
                if (request.user <= 0)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = LeaveMessages.UserIdRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                if (request.leave_type <= 0)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = LeaveMessages.LeaveTypeRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Use organization ID directly from request
                int? organisationId = request.organization;

                // Resolve user to EmployeeId
                var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(request.user);
                if (!employeeId.HasValue)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = LeaveMessages.EmployeeNotFoundForUser,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Use leave type ID directly from request
                int leaveTypeId = request.leave_type;

                // Get current leave balance
                var leaveBalance = await _leaveRepository.GetLeaveBalanceAsync(employeeId.Value, leaveTypeId);
                decimal currentBalance = leaveBalance?.LeaveBalanceValue ?? 0;

                // Calculate duration (if half day, set to 0.5)
                decimal duration = request.is_half_day ? 0.5m : request.duration;

                // Check if sufficient balance
                if (currentBalance < duration)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = string.Format(LeaveMessages.InsufficientLeaveBalance, currentBalance, duration),
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Generate leave request number
                var leaveRequestNumber = await _leaveRepository.GenerateLeaveRequestNumberAsync(organisationId ?? 0);

                // Create leave request object
                var leaveRequest = new LeaveRequest
                {
                    Number = leaveRequestNumber,
                    EmployeeId = employeeId.Value,
                    LeaveTypeId = leaveTypeId,
                    LeaveBalance = currentBalance,
                    FromDate = request.startdate,
                    ToDate = request.enddate,
                    Duration = duration,
                    Description = request.reason,
                    CurrentAction = STATUS_SUBMIT,
                    LeaveRequestStatus = STATUS_ID_SUBMIT,
                    OrganisationId = organisationId,
                    BranchId = request.branch,
                    InsertUserId = request.user,
                    InsertDate = DateTime.Now
                };

                // Insert leave request
                var newId = await _leaveRepository.CreateLeaveRequestAsync(leaveRequest);

                if (newId > 0)
                {
                    // Update leave request with the generated ID for workflow
                    leaveRequest.Id = newId;

                    // Initiate approval workflow (insert into Events, Approval, ScreenNotification tables)
                    try
                    {
                        var tenantId = organisationId ?? 0;
                        if (tenantId > 0)
                        {
                            var workflowResult = await _approvalWorkflowService.InitiateLeaveRequestApprovalAsync(
                                leaveRequest, 
                                request.user, 
                                tenantId);

                            if (workflowResult.Success)
                            {
                                _logger.LogInformation(LogMessages.ApprovalWorkflow.ApprovalWorkflowInitiated, 
                                    newId, workflowResult.EventId);
                            }
                            else
                            {
                                _logger.LogWarning(LogMessages.ApprovalWorkflow.FailedToInitiateWorkflow, 
                                    newId, workflowResult.Message);
                            }
                        }
                    }
                    catch (Exception workflowEx)
                    {
                        // Log but don't fail the leave request creation
                        _logger.LogWarning(workflowEx, LogMessages.ApprovalWorkflow.WorkflowNotConfigured, newId);
                    }

                    return new LeaveRequestResponse
                    {
                        Success = true,
                        Message = LeaveMessages.LeaveRequestSubmittedSuccessfully,
                        Data = new { Id = newId, Number = leaveRequestNumber },
                        TotalRecords = 1
                    };
                }

                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = LeaveMessages.FailedToCreateLeaveRequest,
                    Data = null,
                    TotalRecords = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Leave.ErrorCreatingLeaveRequest);
                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = string.Format(LeaveMessages.ErrorCreatingLeaveRequest, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get leave requests with filters
        /// </summary>
        public async Task<LeaveRequestResponse> GetLeaveRequestsAsync(LeaveRequestGetRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Leave.FetchingLeaveRequests);
                
                // Use organization ID directly from request
                int? organisationId = request.organization;

                // Resolve user to EmployeeId
                int? employeeId = null;
                if (request.user.HasValue)
                {
                    employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(request.user.Value);
                }

                // Use leave type ID directly from request
                int? leaveTypeId = request.leave_type;

                // Map mobile status to database status
                string? status = MapMobileStatusToDbStatus(request.status);

                // Get leave requests
                var leaveRequests = await _leaveRepository.GetLeaveRequestsAsync(
                    organisationId, 
                    employeeId, 
                    leaveTypeId, 
                    status);

                var leaveList = leaveRequests.ToList();

                // Map to response DTOs
                var responseData = leaveList.Select(lr => new LeaveRequestDetailResponse
                {
                    Id = lr.Id,
                    Number = lr.Number,
                    EmployeeId = lr.EmployeeId,
                    EmployeeName = lr.EmployeeName,
                    LeaveTypeId = lr.LeaveTypeId,
                    LeaveTypeName = lr.LeaveTypeName,
                    LeaveBalance = lr.LeaveBalance,
                    FromDate = lr.FromDate,
                    ToDate = lr.ToDate,
                    Duration = lr.Duration,
                    Description = lr.Description,
                    Status = MapStatusIdToString(lr.LeaveRequestStatus),
                    CurrentAction = lr.CurrentAction,
                    InsertDate = lr.InsertDate
                }).ToList();

                return new LeaveRequestResponse
                {
                    Success = true,
                    Message = LeaveMessages.LeaveRequestsFetchedSuccessfully,
                    Data = responseData,
                    TotalRecords = responseData.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Leave.ErrorFetchingLeaveRequests);
                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = string.Format(LeaveMessages.ErrorFetchingLeaveRequests, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get leave request by ID
        /// </summary>
        public async Task<LeaveRequestResponse> GetLeaveRequestByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation(LogMessages.Leave.FetchingLeaveRequestById, id);
                
                var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(id);

                if (leaveRequest == null)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = LeaveMessages.LeaveRequestNotFound,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                var responseData = new LeaveRequestDetailResponse
                {
                    Id = leaveRequest.Id,
                    Number = leaveRequest.Number,
                    EmployeeId = leaveRequest.EmployeeId,
                    EmployeeName = leaveRequest.EmployeeName,
                    LeaveTypeId = leaveRequest.LeaveTypeId,
                    LeaveTypeName = leaveRequest.LeaveTypeName,
                    LeaveBalance = leaveRequest.LeaveBalance,
                    FromDate = leaveRequest.FromDate,
                    ToDate = leaveRequest.ToDate,
                    Duration = leaveRequest.Duration,
                    Description = leaveRequest.Description,
                    Status = MapStatusIdToString(leaveRequest.LeaveRequestStatus),
                    CurrentAction = leaveRequest.CurrentAction,
                    InsertDate = leaveRequest.InsertDate
                };

                return new LeaveRequestResponse
                {
                    Success = true,
                    Message = LeaveMessages.LeaveRequestFetchedSuccessfully,
                    Data = responseData,
                    TotalRecords = 1
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Leave.ErrorFetchingLeaveRequestById);
                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = string.Format(LeaveMessages.ErrorFetchingLeaveRequest, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Approve a leave request
        /// </summary>
        public async Task<LeaveRequestResponse> ApproveLeaveRequestAsync(int id, int approverUserId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Leave.ApprovingLeaveRequest, id);
                
                var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(id);

                if (leaveRequest == null)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = LeaveMessages.LeaveRequestNotFound,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                if (leaveRequest.LeaveRequestStatus == STATUS_ID_APPROVED)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = LeaveMessages.LeaveRequestAlreadyApproved,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Update leave request status
                var updated = await _leaveRepository.UpdateLeaveRequestStatusAsync(id, STATUS_ID_APPROVED, STATUS_APPROVED, approverUserId);

                if (updated)
                {
                    // Deduct leave balance
                    var currentBalance = await _leaveRepository.GetLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId);
                    if (currentBalance != null)
                    {
                        var newBalance = currentBalance.LeaveBalanceValue - leaveRequest.Duration;
                        await _leaveRepository.UpdateLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId, newBalance, approverUserId);

                        // Try to create transaction for approval (optional - table may not exist)
                        try
                        {
                            var transaction = new LeaveTransaction
                            {
                                LeaveTransactionType = (int)LeaveTransactionType.DeductLeave, // DeductLeave = 2
                                EmployeeId = leaveRequest.EmployeeId,
                                LeaveTypeId = leaveRequest.LeaveTypeId,
                                Description = $"Leave approved: {leaveRequest.Number}",
                                LeaveBalance = -leaveRequest.Duration,
                                EffectiveDate = leaveRequest.FromDate,
                                InsertUserId = approverUserId,
                                OrganisationId = leaveRequest.OrganisationId ?? 0,
                                IsActive = true
                            };

                            await _leaveRepository.CreateLeaveTransactionAsync(transaction);
                        }
                        catch (Exception txEx)
                        {
                            // Log but don't fail - transaction logging is optional
                            _logger.LogWarning(txEx, LogMessages.Transaction.TransactionTableNotExist);
                        }
                    }

                    return new LeaveRequestResponse
                    {
                        Success = true,
                        Message = LeaveMessages.LeaveRequestApprovedSuccessfully,
                        Data = new { Id = id },
                        TotalRecords = 1
                    };
                }

                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = LeaveMessages.FailedToApproveLeaveRequest,
                    Data = null,
                    TotalRecords = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Leave.ErrorApprovingLeaveRequest);
                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = string.Format(LeaveMessages.ErrorApprovingLeaveRequest, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Reject a leave request
        /// </summary>
        public async Task<LeaveRequestResponse> RejectLeaveRequestAsync(int id, int rejecterUserId, string? reason)
        {
            try
            {
                _logger.LogInformation(LogMessages.Leave.RejectingLeaveRequest, id);
                
                var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(id);

                if (leaveRequest == null)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = LeaveMessages.LeaveRequestNotFound,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Update leave request status
                var updated = await _leaveRepository.UpdateLeaveRequestStatusAsync(id, STATUS_ID_REJECTED, STATUS_REJECTED, rejecterUserId);

                if (updated)
                {
                    // Note: Rejection doesn't create a transaction since no balance change occurs
                    // If you need to log rejections, you might want to add a separate status field
                    // or use a different approach. For now, we skip transaction creation on rejection.

                    return new LeaveRequestResponse
                    {
                        Success = true,
                        Message = LeaveMessages.LeaveRequestRejectedSuccessfully,
                        Data = new { Id = id },
                        TotalRecords = 1
                    };
                }

                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = LeaveMessages.FailedToRejectLeaveRequest,
                    Data = null,
                    TotalRecords = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Leave.ErrorRejectingLeaveRequest);
                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = string.Format(LeaveMessages.ErrorRejectingLeaveRequest, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Cancel a leave request
        /// </summary>
        public async Task<LeaveRequestResponse> CancelLeaveRequestAsync(int id, int userId, string? reason)
        {
            try
            {
                _logger.LogInformation(LogMessages.Leave.CancellingLeaveRequest, id);
                
                var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(id);

                if (leaveRequest == null)
                {
                    return new LeaveRequestResponse
                    {
                        Success = false,
                        Message = LeaveMessages.LeaveRequestNotFound,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                if (leaveRequest.LeaveRequestStatus == STATUS_ID_APPROVED)
                {
                    // If already approved, restore the balance
                    var currentBalance = await _leaveRepository.GetLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId);
                    if (currentBalance != null)
                    {
                        var newBalance = currentBalance.LeaveBalanceValue + leaveRequest.Duration;
                        await _leaveRepository.UpdateLeaveBalanceAsync(leaveRequest.EmployeeId, leaveRequest.LeaveTypeId, newBalance, userId);

                        // Try to create transaction for cancellation (optional - table may not exist)
                        // When cancelling, we add back the leave balance, so use AddLeave = 1
                        try
                        {
                            var transaction = new LeaveTransaction
                            {
                                LeaveTransactionType = (int)LeaveTransactionType.AddLeave, // AddLeave = 1 (adding back the leave)
                                EmployeeId = leaveRequest.EmployeeId,
                                LeaveTypeId = leaveRequest.LeaveTypeId,
                                Description = $"Leave cancelled: {leaveRequest.Number}. Reason: {reason}",
                                LeaveBalance = leaveRequest.Duration, // Positive value adds back the leave
                                EffectiveDate = DateTime.Now,
                                InsertUserId = userId,
                                OrganisationId = leaveRequest.OrganisationId ?? 0,
                                IsActive = true
                            };

                            await _leaveRepository.CreateLeaveTransactionAsync(transaction);
                        }
                        catch (Exception txEx)
                        {
                            // Log but don't fail - transaction logging is optional
                            _logger.LogWarning(txEx, LogMessages.Transaction.TransactionTableNotExist);
                        }
                    }
                }

                // Update leave request status
                var updated = await _leaveRepository.UpdateLeaveRequestStatusAsync(id, STATUS_ID_CANCELLED, STATUS_CANCELLED, userId);

                if (updated)
                {
                    return new LeaveRequestResponse
                    {
                        Success = true,
                        Message = LeaveMessages.LeaveRequestCancelledSuccessfully,
                        Data = new { Id = id },
                        TotalRecords = 1
                    };
                }

                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = LeaveMessages.FailedToCancelLeaveRequest,
                    Data = null,
                    TotalRecords = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Leave.ErrorCancellingLeaveRequest);
                return new LeaveRequestResponse
                {
                    Success = false,
                    Message = string.Format(LeaveMessages.ErrorCancellingLeaveRequest, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get leave balance for an employee
        /// </summary>
        public async Task<LeaveBalanceResponse> GetLeaveBalanceAsync(int userId, int? organization)
        {
            try
            {
                _logger.LogInformation(LogMessages.Leave.FetchingLeaveBalance, userId);
                
                // Resolve user to EmployeeId
                var employeeId = await _leaveRepository.GetEmployeeIdByUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new LeaveBalanceResponse
                    {
                        Success = false,
                        Message = LeaveMessages.EmployeeNotFoundForUser,
                        Data = null
                    };
                }

                // Get leave balances
                var leaveBalances = await _leaveRepository.GetLeaveBalanceByEmployeeIdAsync(employeeId.Value);
                var balanceList = leaveBalances.ToList();

                var responseData = balanceList.Select(lb => new LeaveBalanceDetail
                {
                    LeaveTypeId = lb.LeaveTypeId,
                    LeaveTypeName = lb.LeaveTypeName,
                    TotalBalance = lb.LeaveBalanceValue,
                    UsedBalance = 0, // Could be calculated from transactions
                    RemainingBalance = lb.LeaveBalanceValue
                }).ToList();

                return new LeaveBalanceResponse
                {
                    Success = true,
                    Message = LeaveMessages.LeaveBalanceFetchedSuccessfully,
                    Data = responseData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Leave.ErrorFetchingLeaveBalance);
                return new LeaveBalanceResponse
                {
                    Success = false,
                    Message = string.Format(LeaveMessages.ErrorFetchingLeaveBalance, ex.Message),
                    Data = null
                };
            }
        }

        /// <summary>
        /// Map mobile app status to database status
        /// </summary>
        private string? MapMobileStatusToDbStatus(string? mobileStatus)
        {
            if (string.IsNullOrEmpty(mobileStatus)) return null;

            return mobileStatus.ToLower() switch
            {
                "pending" => STATUS_SUBMIT,
                "approve" or "approved" => STATUS_APPROVED,
                "reject" or "rejected" => STATUS_REJECTED,
                "cancelled" => STATUS_CANCELLED,
                _ => mobileStatus
            };
        }

        /// <summary>
        /// Map status ID to status string
        /// </summary>
        private string? MapStatusIdToString(int? statusId)
        {
            return statusId switch
            {
                STATUS_ID_SUBMIT => STATUS_SUBMIT,
                STATUS_ID_APPROVED => STATUS_APPROVED,
                STATUS_ID_REJECTED => STATUS_REJECTED,
                STATUS_ID_CANCELLED => STATUS_CANCELLED,
                _ => statusId?.ToString()
            };
        }
    }
}
