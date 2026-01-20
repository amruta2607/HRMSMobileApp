using Dapper;
using Microsoft.Extensions.Logging;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using System;
using System.Data;

namespace MobileWebApi.Services
{
    public class AttendanceOverviewService : IAttendanceOverviewService
    {
        private readonly ISqlConnections _sqlConnections;
        private readonly ILogger<AttendanceOverviewService> _logger;
        private readonly QueryProvider _queryProvider;
        private readonly IEmployeeRepository _employeeRepository;

        public AttendanceOverviewService(
            ISqlConnections sqlConnections,
            ILogger<AttendanceOverviewService> logger, 
            QueryProvider queryProvider,
            IEmployeeRepository employeeRepository)
        {
            _sqlConnections = sqlConnections;
            _logger = logger;
            _queryProvider = queryProvider;
            _employeeRepository = employeeRepository;
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

        public async Task<AttendanceOverviewResponse> GetAttendanceOverviewAsync(AttendanceOverviewRequest request)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(request.UserId);
                if (!employeeId.HasValue)
                {
                    return new AttendanceOverviewResponse
                    {
                        Success = false,
                        Message = "No employee found for the specified UserId.",
                        Data = null
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingAttendanceOverview,
                    employeeId.Value, request.organisationId, request.FromDate, request.ToDate);

                // Validate request
                if (request.organisationId <= 0)
                {
                    return new AttendanceOverviewResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.OrganisationIdRequired,
                        Data = null
                    };
                }

                if (request.FromDate > request.ToDate)
                {
                    return new AttendanceOverviewResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.InvalidDateRange,
                        Data = null
                    };
                }

                // Validate date range does not exceed 7 days
                var fromDate = request.FromDate.Date;
                var toDate = request.ToDate.Date;
                var daysDifference = (toDate - fromDate).Days + 1; // +1 to include both start and end dates

                if (daysDifference > 7)
                {
                    throw new ArgumentException(AttendanceMessages.DateRangeExceedsMaximum);
                }

                _logger.LogInformation(LogMessages.Attendance.AttendanceOverviewEffectiveRange, fromDate, toDate);

                using var connection = _sqlConnections.New("ConnectionString", "Default");

                // 1. Fetch WorkingHours from TenantConfiguration table using OrganisationId
                var workingHours = await GetWorkingHoursAsync(connection, request.organisationId);
                if (workingHours == null)
                {
                    return new AttendanceOverviewResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.WorkingHoursNotFound,
                        Data = null
                    };
                }

                // 2. Calculate working days between FromDate and ToDate excluding days off
                var workingDays = await CalculateWorkingDaysAsync(connection, request.organisationId, fromDate, toDate);

                _logger.LogInformation(LogMessages.Attendance.AttendanceOverviewWorkingHoursAndDays, workingHours.Value, workingDays);

                // 3. ExpectedHours = WorkingHours × WorkingDays
                var expectedHours = workingHours.Value * workingDays;

                // 4. Fetch actual worked hours by summing Duration from Punch table
                var actualHours = await GetActualHoursAsync(connection, employeeId.Value, request.organisationId, fromDate, toDate);

                // 5. ShortfallHours = ExpectedHours − ActualHours (minimum 0)
                var shortfallHours = Math.Max(0, expectedHours - actualHours);

                // Format week string (using effective range, max 7 days)
                var week = $"{fromDate:dd MMM, yyyy} – {toDate:dd MMM, yyyy}";

                return new AttendanceOverviewResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceOverviewFetchedSuccessfully,
                    Data = new AttendanceOverviewData
                    {
                        Week = week,
                        ExpectedHours = Math.Round(expectedHours, 2),
                        ActualHours = Math.Round(actualHours, 2),
                        ShortfallHours = Math.Round(shortfallHours, 2)
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingAttendanceOverview, request.UserId);
                return new AttendanceOverviewResponse
                {
                    Success = false,
                    Message = string.Format(AttendanceMessages.ErrorFetchingAttendanceOverview, ex.Message),
                    Data = null
                };
            }
        }

        private async Task<double?> GetWorkingHoursAsync(IDbConnection connection, int organisationId)
        {
            string query = _queryProvider.Get("AttendanceOverview_GetWorkingHours");

            var result = await connection.QuerySingleOrDefaultAsync<double?>(query, new { TenantId = organisationId });
            return result;
        }

        private async Task<int> CalculateWorkingDaysAsync(IDbConnection connection, int organisationId, DateTime fromDate, DateTime toDate)
        {
            // Get all day off IDs for this organisation
            // Note: DayOffId mapping may vary by system. Common patterns:
            // Pattern 1: 1=Sunday, 2=Monday, 3=Tuesday, 4=Wednesday, 5=Thursday, 6=Friday, 7=Saturday
            // Pattern 2: 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday, 7=Sunday
            string getDayOffsQuery = _queryProvider.Get("AttendanceOverview_GetDayOffs");

            var dayOffIds = await connection.QueryAsync<int>(getDayOffsQuery, new { TenantId = organisationId });
            var dayOffSet = dayOffIds.ToHashSet();

            _logger.LogInformation(LogMessages.Attendance.AttendanceOverviewDayOffIds, organisationId, string.Join(", ", dayOffSet));

            // Calculate working days excluding day offs
            // DayOfWeek enum: Sunday=0, Monday=1, Tuesday=2, Wednesday=3, Thursday=4, Friday=5, Saturday=6
            // DayOffId mapping: 0=Sunday, 1=Monday, 2=Tuesday, 3=Wednesday, 4=Thursday, 5=Friday, 6=Saturday
            // DayOffId directly matches DayOfWeek enum value (no conversion needed)
            
            int workingDays = 0;
            int totalDays = 0;
            var currentDate = fromDate.Date;

            while (currentDate <= toDate.Date)
            {
                totalDays++;
                int dayOfWeek = (int)currentDate.DayOfWeek;
                
                // DayOffId directly matches DayOfWeek enum value (0-6)
                int dayOffId = dayOfWeek;

                // Check if this day is a day off
                bool isDayOff = dayOffSet.Contains(dayOffId);
                if (!isDayOff)
                {
                    workingDays++;
                }

                _logger.LogDebug(LogMessages.Attendance.AttendanceOverviewDateDetails,
                    currentDate.ToString("yyyy-MM-dd"), currentDate.DayOfWeek, dayOfWeek, dayOffId, isDayOff, workingDays);

                currentDate = currentDate.AddDays(1);
            }

            _logger.LogInformation(LogMessages.Attendance.AttendanceOverviewDateRangeCalculation,
                fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"), totalDays, workingDays, totalDays - workingDays);

            return workingDays;
        }

        private async Task<double> GetActualHoursAsync(IDbConnection connection, int employeeId, int organisationId, DateTime fromDate, DateTime toDate)
        {
            string query = _queryProvider.Get("AttendanceOverview_GetActualHours");

            var result = await connection.QuerySingleOrDefaultAsync<double?>(query, new
            {
                EmployeeId = employeeId,
                TenantId = organisationId,
                FromDate = fromDate.Date,
                ToDate = toDate.Date
            });

            return result ?? 0;
        }
    }
}

