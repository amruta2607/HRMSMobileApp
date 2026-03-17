using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using MobileWebApi.Helper;

namespace MobileWebApi.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(
            IAttendanceRepository repo, 
            IEmployeeRepository employeeRepository, 
            IEmployeeService employeeService,
            ILogger<AttendanceService> logger)
        {
            _repo = repo;
            _employeeRepository = employeeRepository;
            _employeeService = employeeService;
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

        public async Task<string> PunchInAsync(PunchInRequest req)
        {
            // Resolve EmployeeId from UserId
            var employeeId = await ResolveEmployeeIdFromUserIdAsync(req.userId);
            if (!employeeId.HasValue)
            {
                _logger.LogWarning(LogMessages.EmployeeResolution.NoEmployeeFoundForUserId, req.userId);
                return EmployeeMessages.EmployeeNotFoundForUserId;
            }

            _logger.LogInformation(LogMessages.Attendance.ProcessingPunchIn, employeeId.Value);

            // Convert incoming UTC timestamps (from mobile) to server local time for storage
            // JSON like "2025-12-15T12:11:08.669Z" is UTC; ToLocalTime will convert to local zone (e.g. IST)
            var punchInLocal = DateTime.SpecifyKind(req.punch_in_time, DateTimeKind.Utc).ToLocalTime();
            var attendanceDateLocal = DateTime.SpecifyKind(req.attendance_date, DateTimeKind.Utc).ToLocalTime().Date;

            // Check if already punched in for this date (prevent double punch-in)
            var existingPunch = await _repo.GetPunchByEmployeeAndDate(employeeId.Value, attendanceDateLocal);

            if (existingPunch != null && existingPunch.PunchIn != null)
            {
                _logger.LogWarning(AttendanceMessages.PunchInAlreadyDone);
                return AttendanceMessages.PunchInAlreadyDone;
            }

          

            // Insert punch-in with location data
            var punchId = await _repo.InsertPunchIn(
                employeeId.Value,
                punchInLocal,
                attendanceDateLocal
            );

            if (punchId > 0)
            {
                _logger.LogInformation(LogMessages.Attendance.PunchInSuccessful, employeeId.Value);
                return AttendanceMessages.PunchInSuccessful;
            }
            
            _logger.LogWarning(AttendanceMessages.PunchInFailed);
            return AttendanceMessages.PunchInFailed;
        }

        public async Task<string> PunchOutAsync(PunchOutRequest req)
        {
            // Resolve EmployeeId from UserId
            var employeeId = await ResolveEmployeeIdFromUserIdAsync(req.userId);
            if (!employeeId.HasValue)
            {
                _logger.LogWarning(LogMessages.EmployeeResolution.NoEmployeeFoundForUserId, req.userId);
                return EmployeeMessages.EmployeeNotFoundForUserId;
            }

            _logger.LogInformation(LogMessages.Attendance.ProcessingPunchOut, employeeId.Value);

            // Convert incoming UTC timestamps (from mobile) to server local time
            var punchOutLocal = DateTime.SpecifyKind(req.punch_out_time, DateTimeKind.Utc).ToLocalTime();
            var attendanceDateLocal = DateTime.SpecifyKind(req.attendance_date, DateTimeKind.Utc).ToLocalTime().Date;
            
            // Check if punch-in exists (prevent punch-out without punch-in)
            var punch = await _repo.GetPunchByEmployeeAndDate(employeeId.Value, attendanceDateLocal);

            if (punch == null || punch.PunchIn == null)
            {
                _logger.LogWarning(AttendanceMessages.CannotPunchOutWithoutPunchIn);
                return AttendanceMessages.CannotPunchOutWithoutPunchIn;
            }

            // Check if already punched out (prevent double punch-out)
            if (punch.PunchOut != null)
            {
                _logger.LogWarning(AttendanceMessages.PunchOutAlreadyDone);
                return AttendanceMessages.PunchOutAlreadyDone;
            }

            // Calculate duration in hours
            double? duration = CalculateDurationInHours(punch.PunchIn, punchOutLocal);

            // Update punch-out with location data
            await _repo.UpdatePunchOut(
                employeeId.Value,
                punchOutLocal,
                attendanceDateLocal,
                duration
            );

            _logger.LogInformation(LogMessages.Attendance.PunchOutSuccessful, employeeId.Value);
            return AttendanceMessages.PunchOutSuccessful;
        }

        private double? CalculateDurationInHours(DateTime? punchIn, DateTime punchOut)
        {
            if (punchIn == null) return null;

            var diff = punchOut - punchIn.Value;
            return Math.Round(diff.TotalHours, 2);
        }

        /// <summary>
        /// Get attendance report based on request parameters (daily or monthly)
        /// </summary>
        public async Task<AttendanceReportResponse> GetAttendanceReportAsync(AttendanceReportRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Attendance.FetchingAttendanceReport);
                
                // Organization ID is now passed directly as int
                
                // Validate request
                if (request.Daily && !request.CalendarDate.HasValue)
                {
                    return new AttendanceReportResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.CalendarDateRequiredForDaily,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                if (request.Monthly && (!request.DateFrom.HasValue || !request.DateTo.HasValue))
                {
                    return new AttendanceReportResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.DateRangeRequiredForMonthly,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Resolve EmployeeId from UserId if UserId is provided
                int? employeeId = null;
                if (request.UserId.HasValue)
                {
                    employeeId = await ResolveEmployeeIdFromUserIdAsync(request.UserId.Value);
                    if (!employeeId.HasValue)
                    {
                        return new AttendanceReportResponse
                        {
                            Success = false,
                            Message = EmployeeMessages.EmployeeNotFoundForUserId,
                            Data = null,
                            TotalRecords = 0
                        };
                    }
                }

                // Create a modified request with EmployeeId for repository (which still uses EmployeeId internally)
                var repoRequest = new AttendanceReportRequest
                {
                    Id = request.Id,
                    BranchId = request.BranchId,
                    Daily = request.Daily,
                    Monthly = request.Monthly,
                    DateFrom = request.DateFrom,
                    DateTo = request.DateTo,
                    CalendarDate = request.CalendarDate,
                    EmployeeId = employeeId, // Use resolved EmployeeId
                    DepartmentId = request.DepartmentId,
                    organization = request.organization
                };

                // Fetch attendance data from repository
                var attendanceData = await _repo.GetAttendanceReportAsync(repoRequest);
                var attendanceList = attendanceData.ToList();

                // Calculate totals
                var totalWorkingHours = attendanceList
                    .Where(a => a.WorkingDuration.HasValue)
                    .Sum(a => a.WorkingDuration.Value);

                var totalWorkingDays = attendanceList
                    .Select(a => a.CalendarDate.Date)
                    .Distinct()
                    .Count();

                return new AttendanceReportResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceReportFetchedSuccessfully,
                    Data = attendanceList,
                    TotalRecords = attendanceList.Count,
                    TotalWorkingDays = totalWorkingDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetAttendanceReport, nameof(GetAttendanceReportAsync), ex, request.UserId);
                return new AttendanceReportResponse
                {
                    Success = false,
                    Message = AttendanceMessages.ErrorFetchingAttendanceReport,
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get attendance report for a specific employee
        /// </summary>
        public async Task<AttendanceReportResponse> GetEmployeeAttendanceAsync(int userId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new AttendanceReportResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFoundForUserId,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingEmployeeAttendance, employeeId.Value, dateFrom, dateTo);
                
                var attendanceData = await _repo.GetEmployeeAttendanceReportAsync(employeeId.Value, dateFrom, dateTo);
                var attendanceList = attendanceData.ToList();

                var totalWorkingHours = attendanceList
                    .Where(a => a.WorkingDuration.HasValue)
                    .Sum(a => a.WorkingDuration.Value);

                var totalWorkingDays = attendanceList
                    .Select(a => a.CalendarDate.Date)
                    .Distinct()
                    .Count();

                return new AttendanceReportResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceReportFetchedSuccessfully,
                    Data = attendanceList,
                    TotalRecords = attendanceList.Count,
                    TotalWorkingDays = totalWorkingDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetEmployeeAttendance, nameof(GetEmployeeAttendanceAsync), ex, userId);
                return new AttendanceReportResponse
                {
                    Success = false,
                    Message = AttendanceMessages.ErrorFetchingAttendanceReport,
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get real-time attendance status for today (or specified date)
        /// Shows all employees who have punched in
        /// </summary>
        public async Task<RealTimeAttendanceResponse> GetRealTimeAttendanceStatusAsync(DateTime? punchDate = null, int? organisationId = null, int? branchId = null, int? departmentId = null)
        {
            try
            {
                var targetDate = punchDate ?? DateTime.Today;
                _logger.LogInformation(LogMessages.Attendance.FetchingRealTimeStatus, targetDate);
                
                var attendanceData = await _repo.GetRealTimeAttendanceStatusAsync(targetDate, organisationId, branchId, departmentId);
                var attendanceList = attendanceData.ToList();

                var punchedIn = attendanceList.Count(a => a.IsPunchedIn);
                var punchedOut = attendanceList.Count(a => a.PunchOut.HasValue);

                return new RealTimeAttendanceResponse
                {
                    Success = true,
                    Message = AttendanceMessages.RealTimeAttendanceFetchedSuccessfully,
                    Data = attendanceList,
                    TotalPunchedIn = punchedIn,
                    TotalPunchedOut = punchedOut,
                    TotalNotPunched = 0 // This would require counting all employees - can be enhanced later
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetRealTimeStatus, nameof(GetRealTimeAttendanceStatusAsync), ex);
                return new RealTimeAttendanceResponse
                {
                    Success = false,
                    Message = AttendanceMessages.ErrorFetchingRealTimeAttendance,
                    Data = null,
                    TotalPunchedIn = 0,
                    TotalPunchedOut = 0,
                    TotalNotPunched = 0
                };
            }
        }

        /// <summary>
        /// Get employees who are currently punched in (no punch out yet)
        /// </summary>
        public async Task<RealTimeAttendanceResponse> GetCurrentlyPunchedInAsync(DateTime? punchDate = null, int? organisationId = null, int? branchId = null, int? departmentId = null)
        {
            try
            {
                var targetDate = punchDate ?? DateTime.Today;
                _logger.LogInformation(LogMessages.Attendance.FetchingCurrentlyPunchedIn, targetDate);
                
                var attendanceData = await _repo.GetCurrentlyPunchedInAsync(targetDate, organisationId, branchId, departmentId);
                var attendanceList = attendanceData.ToList();

                return new RealTimeAttendanceResponse
                {
                    Success = true,
                    Message = AttendanceMessages.CurrentlyPunchedInFetchedSuccessfully,
                    Data = attendanceList,
                    TotalPunchedIn = attendanceList.Count,
                    TotalPunchedOut = 0,
                    TotalNotPunched = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetCurrentlyPunchedIn, nameof(GetCurrentlyPunchedInAsync), ex);
                return new RealTimeAttendanceResponse
                {
                    Success = false,
                    Message = AttendanceMessages.ErrorFetchingRealTimeAttendance,
                    Data = null,
                    TotalPunchedIn = 0,
                    TotalPunchedOut = 0,
                    TotalNotPunched = 0
                };
            }
        }

        /// <summary>
        /// Get attendance by calendar for a specific employee, month, and year
        /// Returns calendar-style data with attendance status for each day
        /// </summary>
        public async Task<CalendarAttendanceResponse> GetAttendanceByCalendarAsync(int userId, int month, int year)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new CalendarAttendanceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFoundForUserId
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingCalendarAttendance, employeeId.Value, month, year);
                
                // Validate parameters
                if (month < 1 || month > 12)
                {
                    return new CalendarAttendanceResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.InvalidMonth
                    };
                }

                if (year < 2000 || year > 2100)
                {
                    return new CalendarAttendanceResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.InvalidYear
                    };
                }

                // Get employee details
                var employee = await _repo.GetEmployeeByIdAsync(employeeId.Value);
                if (employee == null)
                {
                    return new CalendarAttendanceResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound
                    };
                }

                // Get attendance data for the month
                var attendanceData = await _repo.GetAttendanceByCalendarAsync(employeeId.Value, month, year);
                var attendanceDict = attendanceData.ToDictionary(a => a.CalendarDate.Date, a => a);

                // Build calendar data
                var dateFrom = new DateTime(year, month, 1);
                var dateTo = dateFrom.AddMonths(1).AddDays(-1);
                var totalDays = dateTo.Day;
                var today = DateTime.Today;

                var calendarData = new List<CalendarDayAttendance>();
                int presentDays = 0;
                int absentDays = 0;
                int weekendDays = 0;
                double totalWorkingHours = 0;

                for (int day = 1; day <= totalDays; day++)
                {
                    var currentDate = new DateTime(year, month, day);
                    var dayAttendance = new CalendarDayAttendance
                    {
                        Date = currentDate,
                        Day = day,
                        DayName = currentDate.DayOfWeek.ToString(),
                        IsWeekend = currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday
                    };

                    if (dayAttendance.IsWeekend)
                    {
                        dayAttendance.Status = "Weekend";
                        dayAttendance.IsAbsent = false;
                        weekendDays++;
                    }
                    else if (currentDate > today)
                    {
                        dayAttendance.Status = "Future";
                        dayAttendance.IsAbsent = false;
                    }
                    else if (attendanceDict.TryGetValue(currentDate, out var attendance))
                    {
                        dayAttendance.IsPresent = true;
                        dayAttendance.PunchIn = attendance.PunchIn;
                        dayAttendance.PunchOut = attendance.PunchOut;
                        dayAttendance.WorkingHours = attendance.WorkingDuration;
                        dayAttendance.Status = "Present";
                        presentDays++;
                        
                        if (attendance.WorkingDuration.HasValue)
                        {
                            totalWorkingHours += attendance.WorkingDuration.Value;
                        }
                    }
                    else
                    {
                        dayAttendance.IsAbsent = true;
                        dayAttendance.Status = "Absent";
                        absentDays++;
                    }

                    calendarData.Add(dayAttendance);
                }

                var workingDays = totalDays - weekendDays;
                var monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

                return new CalendarAttendanceResponse
                {
                    Success = true,
                    Message = AttendanceMessages.CalendarAttendanceFetchedSuccessfully,
                    EmployeeName = employee.Name,
                    EmployeeNumber = employee.EmployeeNumber,
                    Month = month,
                    MonthName = monthName,
                    Year = year,
                    TotalDays = totalDays,
                    WorkingDays = workingDays,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    LeaveDays = 0, // Can be enhanced to include leave data
                    HolidayDays = 0, // Can be enhanced to include holiday data
                    WeekendDays = weekendDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2),
                    CalendarData = calendarData
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetCalendarAttendance, nameof(GetAttendanceByCalendarAsync), ex, userId);
                return new CalendarAttendanceResponse
                {
                    Success = false,
                    Message = AttendanceMessages.ErrorFetchingCalendarAttendance
                };
            }
        }

        /// <summary>
        /// Get attendance summary for a specific employee within a date range
        /// </summary>
        public async Task<AttendanceSummaryResponse> GetAttendanceSummaryAsync(int organizationId, int userId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new AttendanceSummaryResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFoundForUserId
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingAttendanceSummary, employeeId.Value, fromDate, toDate);
                
                // Validate parameters
                if (fromDate > toDate)
                {
                    return new AttendanceSummaryResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.InvalidDateRange
                    };
                }

                // Get employee details
                var employee = await _repo.GetEmployeeByIdAsync(employeeId.Value);
                if (employee == null)
                {
                    return new AttendanceSummaryResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFound
                    };
                }

                // Get attendance data for the date range
                var attendanceData = await _repo.GetEmployeeAttendanceReportAsync(employeeId.Value, fromDate, toDate);
                var attendanceDict = attendanceData.ToDictionary(a => a.CalendarDate.Date, a => a);

                // Build summary data
                var totalDays = (toDate - fromDate).Days + 1;
                var today = DateTime.Today;

                var attendanceDetails = new List<AttendanceSummaryDetail>();
                int presentDays = 0;
                int absentDays = 0;
                int weekendDays = 0;
                double totalWorkingHours = 0;

                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    var detail = new AttendanceSummaryDetail
                    {
                        Date = date,
                        DayName = date.DayOfWeek.ToString()
                    };

                    bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

                    if (isWeekend)
                    {
                        detail.Status = "Weekend";
                        weekendDays++;
                    }
                    else if (date > today)
                    {
                        detail.Status = "Future";
                    }
                    else if (attendanceDict.TryGetValue(date, out var attendance))
                    {
                        detail.PunchIn = attendance.PunchIn;
                        detail.PunchOut = attendance.PunchOut;
                        detail.WorkingHours = attendance.WorkingDuration;
                        detail.Status = "Present";
                        presentDays++;
                        
                        if (attendance.WorkingDuration.HasValue)
                        {
                            totalWorkingHours += attendance.WorkingDuration.Value;
                        }
                    }
                    else
                    {
                        detail.Status = "Absent";
                        absentDays++;
                    }

                    attendanceDetails.Add(detail);
                }

                var workingDays = totalDays - weekendDays;
                var averageWorkingHours = presentDays > 0 ? Math.Round(totalWorkingHours / presentDays, 2) : 0;

                return new AttendanceSummaryResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceSummaryFetchedSuccessfully,
                    OrganizationId = organizationId,
                    EmployeeName = employee.Name,
                    EmployeeNumber = employee.EmployeeNumber,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalDays = totalDays,
                    WorkingDays = workingDays,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    LeaveDays = 0,
                    HolidayDays = 0,
                    WeekendDays = weekendDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2),
                    AverageWorkingHours = averageWorkingHours,
                    AttendanceDetails = attendanceDetails
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetAttendanceSummary, nameof(GetAttendanceSummaryAsync), ex, userId);
                return new AttendanceSummaryResponse
                {
                    Success = false,
                    Message = AttendanceMessages.ErrorFetchingAttendanceSummary
                };
            }
        }

        /// <summary>
        /// Get attendance reports by organisation ID (current month by default)
        /// </summary>
        public async Task<AttendanceReportResponse> GetAttendanceReportsByOrganisationAsync(int organisationId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Attendance.FetchingAttendanceReportsForOrganisation, organisationId);
                
                // Validate parameters
                if (organisationId <= 0)
                {
                    return new AttendanceReportResponse
                    {
                        Success = false,
                        Message = OrganisationMessages.OrganisationIdRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Get current month's date range
                var today = DateTime.Today;
                var dateFrom = new DateTime(today.Year, today.Month, 1);
                var dateTo = dateFrom.AddMonths(1).AddDays(-1);

                // Fetch attendance data from repository
                var attendanceData = await _repo.GetAttendanceReportsByOrganisationAsync(organisationId, dateFrom, dateTo);
                var attendanceList = attendanceData.ToList();

                // Calculate totals
                var totalWorkingHours = attendanceList
                    .Where(a => a.WorkingDuration.HasValue)
                    .Sum(a => a.WorkingDuration.Value);

                var totalWorkingDays = attendanceList
                    .Select(a => a.CalendarDate.Date)
                    .Distinct()
                    .Count();

                return new AttendanceReportResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceReportFetchedSuccessfully,
                    Data = attendanceList,
                    TotalRecords = attendanceList.Count,
                    TotalWorkingDays = totalWorkingDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetOrganisationReports, nameof(GetAttendanceReportsByOrganisationAsync), ex, organisationId);
                return new AttendanceReportResponse
                {
                    Success = false,
                    Message = AttendanceMessages.ErrorFetchingAttendanceReport,
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Delete attendance record
        /// </summary>
        public async Task<AttendanceDeleteResponse> DeleteAttendanceAsync(int id, int tenantId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Attendance.DeletingAttendanceRecord, id);

                if (id <= 0)
                {
                    return new AttendanceDeleteResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.EmployeeIdRequired,
                        Data = null
                    };
                }

                // Check if punch record exists
                var existingPunch = await _repo.GetPunchByIdAsync(id, tenantId);
                if (existingPunch == null)
                {
                    return new AttendanceDeleteResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.AttendanceNotFound,
                        Data = null
                    };
                }

                // Validate that user has access to this employee's data
                var employee = await _repo.GetEmployeeByIdAsync(existingPunch.EmployeeId);
                if (employee == null)
                {
                    return new AttendanceDeleteResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                var deleted = await _repo.DeletePunchAsync(id, tenantId);

                if (deleted)
                {
                    return new AttendanceDeleteResponse
                    {
                        Success = true,
                        Message = AttendanceMessages.AttendanceDeletedSuccessfully,
                        Data = new { Id = id }
                    };
                }

                return new AttendanceDeleteResponse
                {
                    Success = false,
                    Message = AttendanceMessages.FailedToDeleteAttendance,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.DeleteAttendance, nameof(DeleteAttendanceAsync), ex);
                return new AttendanceDeleteResponse
                {
                    Success = false,
                    Message = AttendanceMessages.FailedToDeleteAttendance,
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get attendance status for an employee on a specific date
        /// </summary>
        public async Task<AttendanceStatusResponse> GetAttendanceStatusAsync(int userId, DateTime date, int tenantId)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new AttendanceStatusResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFoundForUserId,
                        Data = null
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.GettingAttendanceStatus, employeeId.Value, date);

                // Validate parameters
                if (date == default(DateTime))
                {
                    return new AttendanceStatusResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.DateRequired,
                        Data = null
                    };
                }

                // Get punch record with tenant filter
                var punch = await _repo.GetPunchByEmployeeAndDateWithTenant(employeeId.Value, date, tenantId);

                var statusData = new AttendanceStatusData
                {
                    date = date.Date
                };

                if (punch != null)
                {
                    // Attendance is marked
                    statusData.isMarked = true;
                    statusData.isAlreadyMarked = true; // Prevents duplicate punch-in

                    // Determine status based on punch-in/punch-out
                    if (punch.PunchIn.HasValue && punch.PunchOut.HasValue)
                    {
                        statusData.status = AttendanceStatusMessages.Present;
                        statusData.punchIn = punch.PunchIn;
                        statusData.punchOut = punch.PunchOut;
                        statusData.duration = punch.Duration;
                    }
                    else if (punch.PunchIn.HasValue && !punch.PunchOut.HasValue)
                    {
                        statusData.status = AttendanceStatusMessages.Present; // Present but not punched out yet
                        statusData.punchIn = punch.PunchIn;
                        statusData.punchOut = null;
                    }
                    else
                    {
                        statusData.status = AttendanceStatusMessages.Absent; // Record exists but no punch-in
                    }
                }
                else
                {
                    // Attendance not marked
                    statusData.isMarked = false;
                    statusData.isAlreadyMarked = false;
                    statusData.status = AttendanceStatusMessages.NotMarked;
                    statusData.punchIn = null;
                    statusData.punchOut = null;
                }

                return new AttendanceStatusResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceStatusRetrievedSuccessfully,
                    Data = statusData
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetStatus, nameof(GetAttendanceStatusAsync), ex, userId);
                return new AttendanceStatusResponse
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin,
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get today's punch in / punch out logs for the current user
        /// </summary>
        public async Task<TodayPunchLogsResponse> GetTodayPunchLogsAsync(int userId, int tenantId)
        {
            try
            {
                // Resolve employee from UserId
                var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
                if (employee == null)
                {
                    return new TodayPunchLogsResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFoundForUserId,
                        Data = null
                    };
                }

                // Ensure tenant isolation
                if (employee.OrganisationId != tenantId)
                {
                    return new TodayPunchLogsResponse
                    {
                        Success = false,
                        Message = TenantAccessMessages.TenantAccessDenied,
                        Data = null
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingTodayPunchLogs, userId);

                var today = DateTime.Today;
                var logs = await _repo.GetTodayPunchLogsAsync(employee.BiometricNumber, today);
                var logList = logs
                    .OrderBy(l => l.LogDateTime)
                    .ToList();

                return new TodayPunchLogsResponse
                {
                    Success = true,
                    Message = AttendanceMessages.TodayPunchLogsFetchedSuccessfully,
                    Data = logList
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetTodayPunchLogs, nameof(GetTodayPunchLogsAsync), ex, userId);
                return new TodayPunchLogsResponse
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin,
                    Data = null
                };
            }
        }
    }
}
