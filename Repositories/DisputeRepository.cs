using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Resources;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using System.Data;

namespace MobileWebApi.Repositories
{
    public class DisputeRepository : IDisputeRepository
    {
        private const string ManualSource = "Manual";
        private const int AttendanceNotMarkedCategoryId = 4;

        private readonly DapperContext _context;
        private readonly ILogger<DisputeRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public DisputeRepository(
            DapperContext context,
            ILogger<DisputeRepository> logger,
            QueryProvider queryProvider)
        {
            _context = context;
            _logger = logger;
            _queryProvider = queryProvider;
        }

        public async Task<IEnumerable<DisputeCategory>> GetDisputeCategoriesAsync()
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetDisputeCategories");

                return await conn.QueryAsync<DisputeCategory>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetDisputeCategoriesAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeGetDisputeCategoriesDatabaseError}: Failed to fetch dispute categories",
                    ex);
            }
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int employeeId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeById");

                return await conn.QueryFirstOrDefaultAsync<Employee>(query, new { Id = employeeId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeGetEmployeeByIdDatabaseError}: Failed to fetch employee by id for dispute",
                    ex);
            }
        }

        /// <summary>
        /// Matches Web UX_EmployeeDispute_Unique (EmployeeId, DisputeCategoryId, DisputeDate).
        /// </summary>
        public async Task<EmployeeDispute?> GetExistingDisputeAsync(int employeeId, int disputeCategoryId, DateTime disputeDate)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetExistingDispute");

                return await conn.QueryFirstOrDefaultAsync<EmployeeDispute>(query,
                    new
                    {
                        EmployeeId = employeeId,
                        DisputeCategoryId = disputeCategoryId,
                        DisputeDate = disputeDate.Date
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetExistingDisputeAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeGetExistingDisputeDatabaseError}: Failed to fetch existing dispute",
                    ex);
            }
        }

        public async Task<EmployeeDispute?> GetEmployeeDisputeByIdAsync(int disputeId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("GetEmployeeDisputeById");

                return await conn.QueryFirstOrDefaultAsync<EmployeeDispute>(query,
                    new { Id = disputeId, TenantId = tenantId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetEmployeeDisputeByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeGetByIdDatabaseError}: Failed to fetch employee dispute by id",
                    ex);
            }
        }

        public async Task<int> InsertDisputeAsync(EmployeeDispute dispute)
        {
            try
            {
                using var conn = _context.CreateConnection();
                string query = _queryProvider.Get("InsertDispute");

                return await conn.ExecuteScalarAsync<int>(query,
                    new
                    {
                        dispute.EmployeeId,
                        dispute.DisputeCategoryId,
                        DisputeDate = dispute.DisputeDate.Date,
                        dispute.Description,
                        dispute.Status,
                        dispute.CreatedOn,
                        dispute.TenantId,
                        dispute.PunchId,
                        dispute.RequestedPunchInTime,
                        dispute.RequestedPunchOutTime
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(InsertDisputeAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeInsertDisputeDatabaseError}: Failed to insert dispute",
                    ex);
            }
        }

        /// <inheritdoc />
        public async Task<(bool Success, string Message)> ApproveDisputeAndApplyPunchCorrectionAsync(
            int disputeId,
            int tenantId,
            int updateUserId)
        {
            try
            {
                using var conn = _context.CreateConnection();
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                using var tx = conn.BeginTransaction();

                var dispute = await conn.QueryFirstOrDefaultAsync<EmployeeDispute>(
                    _queryProvider.Get("GetEmployeeDisputeById"),
                    new { Id = disputeId, TenantId = tenantId },
                    tx);

                if (dispute == null)
                {
                    tx.Rollback();
                    return (false, DisputeMessages.DisputeNotFound);
                }

                if (string.Equals(dispute.Status, EventStateConstants.Approved, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(dispute.Status, EventStateConstants.Rejected, StringComparison.OrdinalIgnoreCase))
                {
                    tx.Rollback();
                    return (false, DisputeMessages.DisputeAlreadyProcessed);
                }

                // 1) Apply punch correction / create Punch for Attendance Not Marked (before status update)
                var punchResult = await ApplyPunchCorrectionIfNeededAsync(conn, tx, dispute, tenantId, updateUserId);
                if (!punchResult.Success)
                {
                    tx.Rollback();
                    return punchResult;
                }

                // 2) Mark dispute approved only after punch work succeeds
                await conn.ExecuteAsync(
                    _queryProvider.Get("UpdateEmployeeDisputeStatus"),
                    new { Id = disputeId, TenantId = tenantId, Status = EventStateConstants.Approved },
                    tx);

                _logger.LogInformation(LogMessages.Dispute.DisputeStatusUpdated, disputeId, EventStateConstants.Approved);

                tx.Commit();
                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(ApproveDisputeAndApplyPunchCorrectionAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeApproveDatabaseError}: Failed to approve dispute and apply punch correction",
                    ex);
            }
        }

        /// <inheritdoc />
        public async Task<(bool Success, string Message)> RejectDisputeAsync(int disputeId, int tenantId)
        {
            try
            {
                using var conn = _context.CreateConnection();

                var dispute = await conn.QueryFirstOrDefaultAsync<EmployeeDispute>(
                    _queryProvider.Get("GetEmployeeDisputeById"),
                    new { Id = disputeId, TenantId = tenantId });

                if (dispute == null)
                    return (false, DisputeMessages.DisputeNotFound);

                if (string.Equals(dispute.Status, EventStateConstants.Approved, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(dispute.Status, EventStateConstants.Rejected, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, DisputeMessages.DisputeAlreadyProcessed);
                }

                await conn.ExecuteAsync(
                    _queryProvider.Get("UpdateEmployeeDisputeStatus"),
                    new { Id = disputeId, TenantId = tenantId, Status = EventStateConstants.Rejected });

                _logger.LogInformation(LogMessages.Dispute.DisputeStatusUpdated, disputeId, EventStateConstants.Rejected);
                return (true, DisputeMessages.DisputeRejectedSuccessfully);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(RejectDisputeAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.DisputeUpdateStatusDatabaseError}: Failed to reject dispute",
                    ex);
            }
        }

        /// <summary>
        /// Mirrors Web ApproveHelper.ApplyPunchCorrectionIfNeeded.
        /// Updates Punch In/Out from requested times (when present) and recalculates duration.
        /// For Attendance Not Marked with no PunchId, creates a new Punch (or corrects an existing same-date Punch).
        /// </summary>
        private async Task<(bool Success, string Message)> ApplyPunchCorrectionIfNeededAsync(
            IDbConnection conn,
            IDbTransaction tx,
            EmployeeDispute dispute,
            int tenantId,
            int updateUserId)
        {
            var hasRequestedIn = HasPunchTime(dispute.RequestedPunchInTime);
            var hasRequestedOut = HasPunchTime(dispute.RequestedPunchOutTime);

            if (!hasRequestedIn && !hasRequestedOut)
            {
                _logger.LogInformation(LogMessages.Dispute.PunchCorrectionSkipped, dispute.Id, "No requested punch times");
                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }

            string? categoryName = null;
            if (dispute.DisputeCategoryId > 0)
            {
                var categories = await conn.QueryAsync<DisputeCategory>(
                    _queryProvider.Get("GetDisputeCategories"),
                    transaction: tx);
                categoryName = categories.FirstOrDefault(c => c.Id == dispute.DisputeCategoryId)?.CategoryName;
            }

            // Web: "Other" never applies punch correction / creation
            if (!string.IsNullOrEmpty(categoryName)
                && !AttendanceDisputeCategories.AppliesPunchCorrection(categoryName))
            {
                _logger.LogInformation(LogMessages.Dispute.PunchCorrectionSkipped, dispute.Id, $"Category '{categoryName}' does not apply punch correction");
                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }

            _logger.LogInformation(
                "Dispute approval: DisputeId={DisputeId}, EmployeeId={EmployeeId}, " +
                "CategoryId={CategoryId}, CategoryName={CategoryName}, PunchId={PunchId}",
                dispute.Id,
                dispute.EmployeeId,
                dispute.DisputeCategoryId,
                categoryName,
                dispute.PunchId);

            // Attendance Not Marked (CategoryId 4): create or correct Punch even when PunchId is NULL.
            // Must run before GetPunchById — nullable PunchId must not yield PunchRecordNotFound.
            if (dispute.DisputeCategoryId == AttendanceNotMarkedCategoryId)
            {
                // Prefer CategoryId over name; ensure flags resolve if name lookup failed
                categoryName ??= AttendanceDisputeCategories.AttendanceNotMarked;
                return await CreateOrCorrectPunchForAttendanceNotMarkedAsync(
                    conn, tx, dispute, tenantId, updateUserId, categoryName, hasRequestedIn, hasRequestedOut);
            }

            // Other categories require an existing Punch reference
            if (!dispute.PunchId.HasValue || dispute.PunchId.Value <= 0)
            {
                _logger.LogInformation(LogMessages.Dispute.PunchCorrectionSkipped, dispute.Id, "PunchId is missing or zero");
                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }

            var punch = await conn.QueryFirstOrDefaultAsync<Punch>(
                _queryProvider.Get("GetPunchById"),
                new { Id = dispute.PunchId.Value, TenantId = tenantId },
                tx);

            if (punch == null)
                return (false, DisputeMessages.PunchRecordNotFound);

            if (punch.EmployeeId != dispute.EmployeeId)
                return (false, DisputeMessages.InvalidPunchId);

            _logger.LogInformation(LogMessages.Dispute.ApplyingPunchCorrection, dispute.Id, dispute.PunchId.Value);

            var (updatePunchIn, updatePunchOut) = ResolvePunchUpdateFlags(categoryName, hasRequestedIn, hasRequestedOut);

            if (!updatePunchIn && !updatePunchOut)
            {
                _logger.LogInformation(LogMessages.Dispute.PunchCorrectionSkipped, dispute.Id, "No matching punch fields to update for category");
                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }

            DateTime? punchIn = punch.PunchIn;
            DateTime? punchOut = punch.PunchOut;

            if (updatePunchIn)
                punchIn = dispute.RequestedPunchInTime;

            if (updatePunchOut)
                punchOut = dispute.RequestedPunchOutTime;

            var (durationOk, duration, durationError) = TryCalculateDuration(punchIn, punchOut);
            if (!durationOk)
                return (false, durationError!);

            await conn.ExecuteAsync(
                _queryProvider.Get("UpdatePunchForRegularization"),
                new
                {
                    PunchId = dispute.PunchId.Value,
                    TenantId = tenantId,
                    PunchIn = punchIn,
                    PunchOut = punchOut,
                    Duration = duration,
                    UpdatePunchIn = updatePunchIn ? 1 : 0,
                    UpdatePunchOut = updatePunchOut ? 1 : 0,
                    InSource = ManualSource,
                    OutSource = ManualSource,
                    UserId = updateUserId
                },
                tx);

            _logger.LogInformation(
                LogMessages.Dispute.PunchCorrectionApplied,
                dispute.PunchId.Value,
                punchIn,
                punchOut,
                duration);

            return (true, DisputeMessages.DisputeApprovedSuccessfully);
        }

        /// <summary>
        /// Creates a Punch for Attendance Not Marked when none exists for the dispute date.
        /// If a same-date Punch already exists, applies correction instead of inserting a duplicate.
        /// </summary>
        private async Task<(bool Success, string Message)> CreateOrCorrectPunchForAttendanceNotMarkedAsync(
            IDbConnection conn,
            IDbTransaction tx,
            EmployeeDispute dispute,
            int tenantId,
            int updateUserId,
            string? categoryName,
            bool hasRequestedIn,
            bool hasRequestedOut)
        {
            var (updatePunchIn, updatePunchOut) = ResolvePunchUpdateFlags(categoryName, hasRequestedIn, hasRequestedOut);

            if (!updatePunchIn && !updatePunchOut)
            {
                _logger.LogInformation(LogMessages.Dispute.PunchCorrectionSkipped, dispute.Id, "No matching punch fields to update for category");
                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }

            // Prevent duplicate Punch for EmployeeId + DisputeDate + TenantId
            var existingPunch = await conn.QueryFirstOrDefaultAsync<Punch>(
                _queryProvider.Get("GetPunchByEmployeeAndDateWithTenant"),
                new
                {
                    EmployeeId = dispute.EmployeeId,
                    PunchDate = dispute.DisputeDate.Date,
                    TenantId = tenantId
                },
                tx);

            if (existingPunch != null)
            {
                _logger.LogInformation(
                    LogMessages.Dispute.ExistingPunchUsedForAttendanceNotMarked,
                    existingPunch.Id,
                    dispute.Id);

                DateTime? punchIn = existingPunch.PunchIn;
                DateTime? punchOut = existingPunch.PunchOut;

                if (updatePunchIn)
                    punchIn = dispute.RequestedPunchInTime;

                if (updatePunchOut)
                    punchOut = dispute.RequestedPunchOutTime;

                var (durationOk, duration, durationError) = TryCalculateDuration(punchIn, punchOut);
                if (!durationOk)
                    return (false, durationError!);

                await conn.ExecuteAsync(
                    _queryProvider.Get("UpdatePunchForRegularization"),
                    new
                    {
                        PunchId = existingPunch.Id,
                        TenantId = tenantId,
                        PunchIn = punchIn,
                        PunchOut = punchOut,
                        Duration = duration,
                        UpdatePunchIn = updatePunchIn ? 1 : 0,
                        UpdatePunchOut = updatePunchOut ? 1 : 0,
                        InSource = ManualSource,
                        OutSource = ManualSource,
                        UserId = updateUserId
                    },
                    tx);

                await LinkPunchIdToDisputeAsync(conn, tx, dispute.Id, tenantId, existingPunch.Id);

                _logger.LogInformation(
                    LogMessages.Dispute.PunchCorrectionApplied,
                    existingPunch.Id,
                    punchIn,
                    punchOut,
                    duration);

                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }

            // Creating a new Punch requires Punch-In (open punch with Punch-Out only is not supported)
            if (!updatePunchIn || !hasRequestedIn)
                return (false, DisputeMessages.PunchInRequiredForAttendanceNotMarked);

            DateTime? newPunchIn = dispute.RequestedPunchInTime;
            DateTime? newPunchOut = updatePunchOut ? dispute.RequestedPunchOutTime : null;

            var (newDurationOk, newDuration, newDurationError) = TryCalculateDuration(newPunchIn, newPunchOut);
            if (!newDurationOk)
                return (false, newDurationError!);

            _logger.LogInformation(
                LogMessages.Dispute.CreatingPunchForAttendanceNotMarked,
                dispute.Id,
                dispute.DisputeDate.Date);

            var newPunchId = await conn.ExecuteScalarAsync<int>(
                _queryProvider.Get("InsertPunchForRegularization"),
                new
                {
                    EmployeeId = dispute.EmployeeId,
                    PunchDate = dispute.DisputeDate.Date,
                    PunchIn = newPunchIn,
                    PunchOut = newPunchOut,
                    Duration = newDuration,
                    TenantId = tenantId,
                    InsertUserId = updateUserId,
                    InSource = ManualSource,
                    OutSource = updatePunchOut ? ManualSource : null
                },
                tx);

            if (newPunchId <= 0)
                return (false, DisputeMessages.FailedToCreatePunchForDispute);

            await LinkPunchIdToDisputeAsync(conn, tx, dispute.Id, tenantId, newPunchId);

            _logger.LogInformation(
                LogMessages.Dispute.PunchCreatedForAttendanceNotMarked,
                dispute.Id,
                newPunchId,
                newPunchIn,
                newPunchOut,
                newDuration);

            return (true, DisputeMessages.DisputeApprovedSuccessfully);
        }

        private async Task LinkPunchIdToDisputeAsync(
            IDbConnection conn,
            IDbTransaction tx,
            int disputeId,
            int tenantId,
            int punchId)
        {
            await conn.ExecuteAsync(
                _queryProvider.Get("UpdateEmployeeDisputePunchId"),
                new { Id = disputeId, TenantId = tenantId, PunchId = punchId },
                tx);
        }

        private static (bool UpdatePunchIn, bool UpdatePunchOut) ResolvePunchUpdateFlags(
            string? categoryName,
            bool hasRequestedIn,
            bool hasRequestedOut)
        {
            if (!string.IsNullOrEmpty(categoryName))
            {
                return (
                    hasRequestedIn && AttendanceDisputeCategories.UpdatesPunchIn(categoryName),
                    hasRequestedOut && AttendanceDisputeCategories.UpdatesPunchOut(categoryName));
            }

            return (hasRequestedIn, hasRequestedOut);
        }

        /// <summary>
        /// Same approach as AttendanceService.CalculateDurationInMinutes.
        /// </summary>
        private static (bool Success, double? Duration, string? Error) TryCalculateDuration(
            DateTime? punchIn,
            DateTime? punchOut)
        {
            if (!punchIn.HasValue || !punchOut.HasValue)
                return (true, null, null);

            if (punchOut.Value <= punchIn.Value)
                return (false, null, DisputeMessages.InvalidApprovedPunchTimes);

            var duration = Math.Round((punchOut.Value - punchIn.Value).TotalMinutes, 2);
            if (duration < 0)
                return (false, null, DisputeMessages.InvalidApprovedPunchTimes);

            return (true, duration, null);
        }

        private static bool HasPunchTime(DateTime? value) =>
            value.HasValue && value.Value != default;
    }
}
