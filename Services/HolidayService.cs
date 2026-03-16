using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using MobileWebApi.Helper;

namespace MobileWebApi.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _holidayRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<HolidayService> _logger;

        public HolidayService(
            IHolidayRepository holidayRepository,
            IUserRepository userRepository,
            ILogger<HolidayService> logger)
        {
            _holidayRepository = holidayRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Add a new holiday
        /// </summary>
        public async Task<HolidayResponse> AddHolidayAsync(HolidayCreateRequest request, int tenantId, int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Holiday.CreatingHoliday, request.holiday_name);

                // Validate required fields
                if (string.IsNullOrWhiteSpace(request.holiday_name))
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.HolidayNameRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                if (request.date == default(DateTime))
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.HolidayDateRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Create holiday entity
                var holiday = new Holiday
                {
                    Name = request.holiday_name,
                    Date = request.date.Date, // Store only date part
                    Description = request.description,
                    TenantId = tenantId,
                    InsertUserId = userId,
                    InsertDate = DateTime.Now,
                    IsActive = true
                };

                var newId = await _holidayRepository.CreateHolidayAsync(holiday);

                if (newId > 0)
                {
                    return new HolidayResponse
                    {
                        Success = true,
                        Message = HolidayMessages.HolidayCreatedSuccessfully,
                        Data = new { Id = newId, Name = holiday.Name },
                        TotalRecords = 1
                    };
                }

                return new HolidayResponse
                {
                    Success = false,
                    Message = HolidayMessages.FailedToCreateHoliday,
                    Data = null,
                    TotalRecords = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Holiday.AddHoliday, nameof(AddHolidayAsync), ex, userId);
                return new HolidayResponse
                {
                    Success = false,
                    Message = $"{HolidayMessages.ErrorCreatingHoliday} (Error Code: {ExceptionCodes.Holiday.AddHoliday})",
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get holidays with filters (OrdiNet compatible)
        /// </summary>
        public async Task<HolidayResponse> GetHolidaysWithFiltersAsync(int? userId, int? organizationId, int? year)
        {
            try
            {
                // Determine tenant ID from organization or user
                int tenantId = organizationId ?? 0;
                
                // If tenant not specified but user is provided, get it from user
                if (tenantId <= 0 && userId.HasValue)
                {
                    var user = await _userRepository.GetUserByIdAsync(userId.Value);
                    if (user != null)
                    {
                        tenantId = user.OrganisationId;
                    }
                    else
                    {
                        _logger.LogWarning(LogMessages.HolidayAdditional.UserNotFoundForUserId, userId.Value);
                        return new HolidayResponse
                        {
                            Success = false,
                            Message = "User not found.",
                            Data = null,
                            TotalRecords = 0
                        };
                    }
                }

                // If still no tenant ID found, this shouldn't happen in normal flow
                // but we'll return an error for clarity
                if (tenantId <= 0)
                {
                    _logger.LogWarning(LogMessages.HolidayAdditional.OrganizationIdRequired);
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = "Organization ID is required. Please provide either organization_id or user_id parameter.",
                        Data = null,
                        TotalRecords = 0
                    };
                }

                _logger.LogInformation(LogMessages.Holiday.FetchingHolidays, tenantId);

                var holidays = await _holidayRepository.GetHolidaysWithFiltersAsync(tenantId, year);
                var holidayList = holidays.ToList();

                var responseData = holidayList.Select(h => new HolidayDetailResponse
                {
                    Id = h.Id,
                    Name = h.Name,
                    Date = h.Date,
                    Description = h.Description,
                    InsertDate = h.InsertDate,
                    InsertUserId = h.InsertUserId,
                    UpdateDate = h.UpdateDate,
                    UpdateUserId = h.UpdateUserId,
                    TenantId = h.TenantId,
                    IsActive = h.IsActive
                }).ToList();

                return new HolidayResponse
                {
                    Success = true,
                    Message = HolidayMessages.HolidaysFetchedSuccessfully,
                    Data = responseData,
                    TotalRecords = responseData.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Holiday.GetHolidaysWithFilters, nameof(GetHolidaysWithFiltersAsync), ex, userId);
                return new HolidayResponse
                {
                    Success = false,
                    Message = $"{HolidayMessages.ErrorFetchingHolidays} (Error Code: {ExceptionCodes.Holiday.GetHolidaysWithFilters})",
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get all holidays for the tenant
        /// </summary>
        public async Task<HolidayResponse> GetAllHolidaysAsync(int tenantId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Holiday.FetchingHolidays, tenantId);

                var holidays = await _holidayRepository.GetAllHolidaysAsync(tenantId);
                var holidayList = holidays.ToList();

                var responseData = holidayList.Select(h => new HolidayDetailResponse
                {
                    Id = h.Id,
                    Name = h.Name,
                    Date = h.Date,
                    Description = h.Description,
                    InsertDate = h.InsertDate,
                    InsertUserId = h.InsertUserId,
                    UpdateDate = h.UpdateDate,
                    UpdateUserId = h.UpdateUserId,
                    TenantId = h.TenantId,
                    IsActive = h.IsActive
                }).ToList();

                return new HolidayResponse
                {
                    Success = true,
                    Message = HolidayMessages.HolidaysFetchedSuccessfully,
                    Data = responseData,
                    TotalRecords = responseData.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Holiday.GetAllHolidays, nameof(GetAllHolidaysAsync), ex);
                return new HolidayResponse
                {
                    Success = false,
                    Message = $"{HolidayMessages.ErrorFetchingHolidays} (Error Code: {ExceptionCodes.Holiday.GetAllHolidays})",
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Update a holiday
        /// </summary>
        public async Task<HolidayResponse> UpdateHolidayAsync(HolidayUpdateRequest request, int tenantId, int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Holiday.UpdatingHoliday, request.Id);

                // Validate ID
                if (request.Id <= 0)
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.InvalidHolidayId,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Check if holiday exists
                var existingHoliday = await _holidayRepository.GetHolidayByIdAsync(request.Id, tenantId);
                if (existingHoliday == null)
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.HolidayNotFound,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Update only provided fields
                var holiday = new Holiday
                {
                    Id = request.Id,
                    Name = request.Name ?? existingHoliday.Name,
                    Date = request.Date ?? existingHoliday.Date,
                    Description = request.Description ?? existingHoliday.Description,
                    TenantId = tenantId,
                    UpdateUserId = userId,
                    UpdateDate = DateTime.Now,
                    InsertUserId = existingHoliday.InsertUserId,
                    InsertDate = existingHoliday.InsertDate,
                    IsActive = existingHoliday.IsActive
                };

                var updated = await _holidayRepository.UpdateHolidayAsync(holiday);

                if (updated)
                {
                    return new HolidayResponse
                    {
                        Success = true,
                        Message = HolidayMessages.HolidayUpdatedSuccessfully,
                        Data = new { Id = request.Id },
                        TotalRecords = 1
                    };
                }

                return new HolidayResponse
                {
                    Success = false,
                    Message = HolidayMessages.FailedToUpdateHoliday,
                    Data = null,
                    TotalRecords = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Holiday.UpdateHoliday, nameof(UpdateHolidayAsync), ex, userId);
                return new HolidayResponse
                {
                    Success = false,
                    Message = $"{HolidayMessages.ErrorUpdatingHoliday} (Error Code: {ExceptionCodes.Holiday.UpdateHoliday})",
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Delete a holiday
        /// </summary>
        public async Task<HolidayResponse> DeleteHolidayAsync(int id, int tenantId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Holiday.DeletingHoliday, id);

                if (id <= 0)
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.InvalidHolidayId,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Check if holiday exists
                var existingHoliday = await _holidayRepository.GetHolidayByIdAsync(id, tenantId);
                if (existingHoliday == null)
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.HolidayNotFound,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                var deleted = await _holidayRepository.DeleteHolidayAsync(id, tenantId);

                if (deleted)
                {
                    return new HolidayResponse
                    {
                        Success = true,
                        Message = HolidayMessages.HolidayDeletedSuccessfully,
                        Data = new { Id = id },
                        TotalRecords = 1
                    };
                }

                return new HolidayResponse
                {
                    Success = false,
                    Message = HolidayMessages.FailedToDeleteHoliday,
                    Data = null,
                    TotalRecords = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Holiday.DeleteHoliday, nameof(DeleteHolidayAsync), ex);
                return new HolidayResponse
                {
                    Success = false,
                    Message = $"{HolidayMessages.ErrorDeletingHoliday} (Error Code: {ExceptionCodes.Holiday.DeleteHoliday})",
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Add multiple holidays in bulk
        /// </summary>
        public async Task<HolidayResponse> AddBulkHolidaysAsync(HolidayBulkCreateRequest request, int tenantId, int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Holiday.CreatingBulkHolidays, request.Holidays.Count);

                if (request.Holidays == null || !request.Holidays.Any())
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.HolidaysListRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Validate all holidays before inserting
                var validationErrors = new List<string>();
                var holidays = new List<Holiday>();

                for (int i = 0; i < request.Holidays.Count; i++)
                {
                    var req = request.Holidays[i];
                    if (string.IsNullOrWhiteSpace(req.holiday_name))
                    {
                        validationErrors.Add($"Holiday at index {i + 1}: Holiday name is required.");
                        continue;
                    }

                    if (req.date == default(DateTime))
                    {
                        validationErrors.Add($"Holiday at index {i + 1}: Holiday date is required.");
                        continue;
                    }

                    holidays.Add(new Holiday
                    {
                        Name = req.holiday_name,
                        Date = req.date.Date,
                        Description = req.description,
                        TenantId = tenantId,
                        InsertUserId = userId,
                        InsertDate = DateTime.Now,
                        IsActive = true
                    });
                }

                if (validationErrors.Any())
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = string.Join(" ", validationErrors),
                        Data = null,
                        TotalRecords = 0
                    };
                }

                var insertedCount = await _holidayRepository.BulkCreateHolidaysAsync(holidays);

                return new HolidayResponse
                {
                    Success = true,
                    Message = string.Format(HolidayMessages.BulkHolidaysCreatedSuccessfully, insertedCount),
                    Data = new { InsertedCount = insertedCount },
                    TotalRecords = insertedCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Holiday.AddBulkHolidays, nameof(AddBulkHolidaysAsync), ex, userId);
                return new HolidayResponse
                {
                    Success = false,
                    Message = $"{HolidayMessages.ErrorCreatingBulkHolidays} (Error Code: {ExceptionCodes.Holiday.AddBulkHolidays})",
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Update holiday date only (simple form data update)
        /// </summary>
        public async Task<HolidayResponse> UpdateHolidayDateAsync(int id, DateTime date, int tenantId, int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Holiday.UpdatingHoliday, id);

                // Validate ID
                if (id <= 0)
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.InvalidHolidayId,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Validate date
                if (date == default(DateTime))
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.HolidayDateRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Check if holiday exists
                var existingHoliday = await _holidayRepository.GetHolidayByIdAsync(id, tenantId);
                if (existingHoliday == null)
                {
                    return new HolidayResponse
                    {
                        Success = false,
                        Message = HolidayMessages.HolidayNotFound,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Update only the date field
                var holiday = new Holiday
                {
                    Id = id,
                    Name = existingHoliday.Name,
                    Date = date.Date, // Store only date part
                    Description = existingHoliday.Description,
                    TenantId = tenantId,
                    UpdateUserId = userId,
                    UpdateDate = DateTime.Now,
                    InsertUserId = existingHoliday.InsertUserId,
                    InsertDate = existingHoliday.InsertDate,
                    IsActive = existingHoliday.IsActive
                };

                var updated = await _holidayRepository.UpdateHolidayAsync(holiday);

                if (updated)
                {
                    return new HolidayResponse
                    {
                        Success = true,
                        Message = HolidayMessages.HolidayUpdatedSuccessfully,
                        Data = new { Id = id, Date = date.Date },
                        TotalRecords = 1
                    };
                }

                return new HolidayResponse
                {
                    Success = false,
                    Message = HolidayMessages.FailedToUpdateHoliday,
                    Data = null,
                    TotalRecords = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Holiday.UpdateHolidayDate, nameof(UpdateHolidayDateAsync), ex, userId);
                return new HolidayResponse
                {
                    Success = false,
                    Message = $"{HolidayMessages.ErrorUpdatingHoliday} (Error Code: {ExceptionCodes.Holiday.UpdateHolidayDate})",
                    Data = null,
                    TotalRecords = 0
                };
            }
        }
    }
}

