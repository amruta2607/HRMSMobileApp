using System.Data;
using System.Globalization;
using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Models.Responses;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    /// <summary>
    /// Dapper-based repository for PunchTracking table operations.
    /// </summary>
    public class PunchTrackingRepository : IPunchTrackingRepository
    {
        private const string DirectionIn = "IN";
        private const string DirectionOut = "OUT";

        private readonly DapperContext _context;
        private readonly QueryProvider _queryProvider;
        private readonly ITenantContext _tenantContext;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<PunchTrackingRepository> _logger;

        public PunchTrackingRepository(
            DapperContext context,
            QueryProvider queryProvider,
            ITenantContext tenantContext,
            IEmployeeRepository employeeRepository,
            ILogger<PunchTrackingRepository> logger)
        {
            _context = context;
            _queryProvider = queryProvider;
            _tenantContext = tenantContext;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<int> InsertPunchTrackingAsync(PunchTracking tracking)
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                var id = await InsertPunchTrackingAsync(tracking, connection, transaction);
                transaction.Commit();
                return id;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Failed to insert punch tracking for PunchId {PunchId}", tracking.PunchId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<int> InsertPunchTrackingAsync(
            PunchTracking tracking,
            IDbConnection connection,
            IDbTransaction transaction)
        {
            var query = _queryProvider.Get("InsertPunchTracking");

            return await connection.ExecuteScalarAsync<int>(
                query,
                new
                {
                    tracking.TenantId,
                    tracking.EmployeeId,
                    tracking.PunchId,
                    PunchDate = tracking.PunchDate.Date,
                    tracking.Direction,
                    PunchIn = tracking.PunchIn,
                    PunchOut = tracking.PunchOut,
                    Duration = ToSqlTimeDuration(tracking.Duration),
                    tracking.InsertUserId,
                    tracking.InSource,
                    tracking.OutSource,
                    tracking.CoordinateIn,
                    tracking.CoordinateOut,
                    tracking.LinkIn,
                    tracking.LinkOut,
                    tracking.PunchInImage,
                    tracking.PunchOutImage,
                    tracking.Manual,
                    tracking.PunchOutReason
                },
                transaction);
        }

        /// <summary>
        /// Maps punch clock time to SQL time column.
        /// </summary>
        private static TimeSpan? ToSqlTime(DateTime? dateTime) =>
            dateTime?.TimeOfDay;

        /// <summary>
        /// Maps duration in minutes to SQL time column.
        /// </summary>
        private static TimeSpan? ToSqlTimeDuration(double? durationMinutes) =>
            durationMinutes.HasValue ? TimeSpan.FromMinutes(durationMinutes.Value) : null;

        /// <inheritdoc />
        public async Task<PunchTracking?> GetLastPunchTrackingAsync(int employeeId, int tenantId, DateTime punchDate)
        {
            using var connection = _context.CreateConnection();
            var query = _queryProvider.Get("GetLastPunchTracking");

            return await connection.QueryFirstOrDefaultAsync<PunchTracking>(query, new
            {
                EmployeeId = employeeId,
                TenantId = tenantId,
                PunchDate = punchDate.Date
            });
        }

        /// <inheritdoc />
        public async Task<PunchTracking?> GetLastUnmatchedPunchInAsync(int punchId)
        {
            using var connection = _context.CreateConnection();
            var query = _queryProvider.Get("GetLastUnmatchedPunchIn");

            return await connection.QueryFirstOrDefaultAsync<PunchTracking>(query, new { PunchId = punchId });
        }

        /// <inheritdoc />
        public async Task<double> GetCompletedPunchTrackingDurationSumAsync(int punchId)
        {
            using var connection = _context.CreateConnection();
            var query = _queryProvider.Get("GetCompletedPunchTrackingDurationSum");

            return await connection.ExecuteScalarAsync<double>(query, new { PunchId = punchId });
        }

        /// <inheritdoc />
        public async Task<PunchTrackingTimelineResult> GetPunchTrackingTimelineAsync(int punchId)
        {
            var userId = _tenantContext.UserId;
            if (!userId.HasValue)
            {
                return PunchTrackingTimelineResult.Unauthorized();
            }

            int tenantId;
            try
            {
                tenantId = _tenantContext.GetRequiredOrganisationId();
            }
            catch (UnauthorizedAccessException)
            {
                return PunchTrackingTimelineResult.Unauthorized();
            }

            var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId.Value);
            if (employee == null || employee.OrganisationId != tenantId)
            {
                return PunchTrackingTimelineResult.Unauthorized();
            }

            var punch = await GetPunchByIdAsync(punchId, tenantId);
            if (punch == null)
            {
                return PunchTrackingTimelineResult.NotFound();
            }

            if (punch.EmployeeId != employee.Id)
            {
                _logger.LogWarning(
                    "Punch tracking access denied. UserId {UserId} attempted to access PunchId {PunchId} belonging to EmployeeId {EmployeeId}",
                    userId.Value,
                    punchId,
                    punch.EmployeeId);

                return PunchTrackingTimelineResult.Forbidden();
            }

            using var connection = _context.CreateConnection();
            var query = _queryProvider.Get("GetPunchTrackingTimeline");

            var rows = (await connection.QueryAsync<PunchTrackingTimelineRowDto>(query, new
            {
                PunchId = punchId,
                EmployeeId = employee.Id,
                TenantId = tenantId
            })).ToList();

            var timeline = rows.Select(row => new PunchTrackingTimelineItemDto
            {
                Id = row.Id,
                Direction = row.Direction,
                Time = row.PunchTime?.ToString("HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty,
                Source = row.Source,
                Coordinate = row.Coordinate,
                Address = row.Address,
                PunchInImage = row.PunchInImage,
                PunchOutImage=row.PunchOutImage,
                Manual = row.Manual,
                Remarks = row.Remarks
            }).ToList();

            var firstPunchIn = rows
                .Where(r => string.Equals(r.Direction, DirectionIn, StringComparison.OrdinalIgnoreCase) && r.PunchTime.HasValue)
                .OrderBy(r => r.PunchTime)
                .Select(r => r.PunchTime!.Value.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
                .FirstOrDefault();

            var lastPunchOut = rows
                .Where(r => string.Equals(r.Direction, DirectionOut, StringComparison.OrdinalIgnoreCase) && r.PunchTime.HasValue)
                .OrderByDescending(r => r.PunchTime)
                .Select(r => r.PunchTime!.Value.ToString("HH:mm:ss", CultureInfo.InvariantCulture))
                .FirstOrDefault();

            var response = new PunchTrackingTimelineResponse
            {
                PunchId = punch.Id,
                EmployeeId = employee.Id,
                EmployeeName = rows.FirstOrDefault()?.EmployeeName ?? employee.Name,
                PunchDate = punch.PunchDate.Date,
                FirstPunchIn = firstPunchIn,
                LastPunchOut = lastPunchOut,
                TotalEntries = timeline.Count,
                Timeline = timeline
            };

            return PunchTrackingTimelineResult.Success(response);
        }

        private async Task<Punch?> GetPunchByIdAsync(int punchId, int tenantId)
        {
            using var connection = _context.CreateConnection();
            var query = _queryProvider.Get("GetPunchById");

            return await connection.QueryFirstOrDefaultAsync<Punch>(query, new
            {
                Id = punchId,
                TenantId = tenantId
            });
        }
    }
}
