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

                // 1) Mark dispute approved
                await conn.ExecuteAsync(
                    _queryProvider.Get("UpdateEmployeeDisputeStatus"),
                    new { Id = disputeId, TenantId = tenantId, Status = EventStateConstants.Approved },
                    tx);

                _logger.LogInformation(LogMessages.Dispute.DisputeStatusUpdated, disputeId, EventStateConstants.Approved);

                // 2) Apply punch correction (Web ApplyPunchCorrectionIfNeeded)
                var punchResult = await ApplyPunchCorrectionIfNeededAsync(conn, tx, dispute, tenantId, updateUserId);
                if (!punchResult.Success)
                {
                    tx.Rollback();
                    return punchResult;
                }

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
        /// </summary>
        private async Task<(bool Success, string Message)> ApplyPunchCorrectionIfNeededAsync(
            IDbConnection conn,
            IDbTransaction tx,
            EmployeeDispute dispute,
            int tenantId,
            int updateUserId)
        {
            if (dispute.PunchId <= 0)
            {
                _logger.LogInformation(LogMessages.Dispute.PunchCorrectionSkipped, dispute.Id, "PunchId is missing or zero");
                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }

            var hasRequestedIn = dispute.RequestedPunchInTime.HasValue
                && dispute.RequestedPunchInTime.Value != default;
            var hasRequestedOut = dispute.RequestedPunchOutTime.HasValue
                && dispute.RequestedPunchOutTime.Value != default;

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

            // Web: "Other" never applies punch correction
            if (!string.IsNullOrEmpty(categoryName)
                && !AttendanceDisputeCategories.AppliesPunchCorrection(categoryName))
            {
                _logger.LogInformation(LogMessages.Dispute.PunchCorrectionSkipped, dispute.Id, $"Category '{categoryName}' does not apply punch correction");
                return (true, DisputeMessages.DisputeApprovedSuccessfully);
            }

            var punch = await conn.QueryFirstOrDefaultAsync<Punch>(
                _queryProvider.Get("GetPunchById"),
                new { Id = dispute.PunchId, TenantId = tenantId },
                tx);

            if (punch == null)
                return (false, DisputeMessages.PunchRecordNotFound);

            if (punch.EmployeeId != dispute.EmployeeId)
                return (false, DisputeMessages.InvalidPunchId);

            _logger.LogInformation(LogMessages.Dispute.ApplyingPunchCorrection, dispute.Id, dispute.PunchId);

            // User rules: update only fields that have requested values.
            // When category is known, also respect Web UpdatesPunchIn / UpdatesPunchOut.
            bool updatePunchIn;
            bool updatePunchOut;

            if (!string.IsNullOrEmpty(categoryName))
            {
                updatePunchIn = hasRequestedIn && AttendanceDisputeCategories.UpdatesPunchIn(categoryName);
                updatePunchOut = hasRequestedOut && AttendanceDisputeCategories.UpdatesPunchOut(categoryName);
            }
            else
            {
                updatePunchIn = hasRequestedIn;
                updatePunchOut = hasRequestedOut;
            }

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

            double? duration = null;
            if (punchIn.HasValue && punchOut.HasValue)
            {
                if (punchOut.Value <= punchIn.Value)
                    return (false, DisputeMessages.InvalidApprovedPunchTimes);

                // Same approach as AttendanceService.CalculateDurationInMinutes
                duration = Math.Round((punchOut.Value - punchIn.Value).TotalMinutes, 2);
                if (duration < 0)
                    return (false, DisputeMessages.InvalidApprovedPunchTimes);
            }

            await conn.ExecuteAsync(
                _queryProvider.Get("UpdatePunchForRegularization"),
                new
                {
                    PunchId = dispute.PunchId,
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
                dispute.PunchId,
                punchIn,
                punchOut,
                duration);

            return (true, DisputeMessages.DisputeApprovedSuccessfully);
        }
    }
}
