using Dapper;
using Microsoft.Extensions.Logging;
using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Helper;
using System;
using System.Data;
using System.Linq;

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
                        Message = EmployeeMessages.EmployeeNotFoundForUserId,
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
                var workingDays = await CalculateWorkingDaysAsync(connection, employeeId.Value, request.organisationId, fromDate, toDate);

                _logger.LogInformation(LogMessages.Attendance.AttendanceOverviewWorkingHoursAndDays, workingHours.Value, workingDays);

                // 3. ExpectedHours = WorkingHours × WorkingDays
                var expectedHours = workingHours.Value * workingDays;

                // 4. Actual worked hours (elapsed from PunchIn to PunchOut)
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
                _logger.LogException(ExceptionCodes.AttendanceOverview.GetOverview, nameof(GetAttendanceOverviewAsync), ex, request.UserId);
                return new AttendanceOverviewResponse
                {
                    Success = false,
                    Message = $"Something went wrong. Please contact the administration team. (Error Code: {ExceptionCodes.AttendanceOverview.GetOverview})",
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

        private async Task<int> CalculateWorkingDaysAsync(
            IDbConnection connection,
            int employeeId,
            int organisationId,
            DateTime fromDate,
            DateTime toDate)
        {
            var weekOffConfig = await GetWeekOffConfigurationAsync(connection, employeeId, organisationId);

            _logger.LogInformation(
                LogMessages.Attendance.AttendanceOverviewDayOffIds,
                organisationId,
                string.Join(", ", weekOffConfig.CompleteWeekOffDays));

            int workingDays = 0;
            int totalDays = 0;
            var currentDate = fromDate.Date;

            while (currentDate <= toDate.Date)
            {
                totalDays++;
                int dayOfWeek = (int)currentDate.DayOfWeek;
                bool isDayOff = WeekOffHelper.IsWeeklyOff(currentDate, weekOffConfig);

                if (!isDayOff)
                {
                    workingDays++;
                }

                _logger.LogDebug(LogMessages.Attendance.AttendanceOverviewDateDetails,
                    currentDate.ToString("yyyy-MM-dd"), currentDate.DayOfWeek, dayOfWeek, dayOfWeek, isDayOff, workingDays);

                currentDate = currentDate.AddDays(1);
            }

            _logger.LogInformation(LogMessages.Attendance.AttendanceOverviewDateRangeCalculation,
                fromDate.ToString("yyyy-MM-dd"), toDate.ToString("yyyy-MM-dd"), totalDays, workingDays, totalDays - workingDays);

            return workingDays;
        }

        private async Task<WeekOffConfiguration> GetWeekOffConfigurationAsync(
            IDbConnection connection,
            int employeeId,
            int organisationId)
        {
            string employeeConfigQuery = _queryProvider.Get("AttendanceOverview_GetEmployeeLevelAttendance");
            var employeeConfig = await connection.QuerySingleOrDefaultAsync<EmployeeLevelAttendanceWeekOffDto>(
                employeeConfigQuery,
                new { EmployeeId = employeeId, TenantId = organisationId });

            if (employeeConfig != null)
            {
                var completeWeekOffDays = WeekOffHelper.ParseCompleteWeekOffs(employeeConfig.WeekOffList);
                List<PartialWeekOffDayItem>? partialWeekOffDays = WeekOffHelper.HasPartialWeekOffJson(employeeConfig.PartialWeekOffJson)
                    ? WeekOffHelper.ParsePartialWeekOffs(employeeConfig.PartialWeekOffJson)
                    : null;

                return WeekOffHelper.BuildConfiguration(completeWeekOffDays, partialWeekOffDays);
            }

            string getDayOffsQuery = _queryProvider.Get("AttendanceOverview_GetDayOffs");
            var tenantDayOffIds = (await connection.QueryAsync<int>(getDayOffsQuery, new { TenantId = organisationId })).ToList();

            string partialWeekOffQuery = _queryProvider.Get("AttendanceOverview_GetTenantPartialWeekOffDays");
            var tenantPartialWeekOffDays = (await connection.QueryAsync<PartialWeekOffDayItem>(
                partialWeekOffQuery,
                new { TenantId = organisationId })).ToList();

            List<PartialWeekOffDayItem>? partialWeekOffDaysForTenant = tenantPartialWeekOffDays.Count > 0
                ? tenantPartialWeekOffDays
                : null;

            return WeekOffHelper.BuildConfiguration(tenantDayOffIds, partialWeekOffDaysForTenant);
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

