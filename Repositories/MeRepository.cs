using System.Diagnostics;
using Dapper;
using MobileWebApi.Constants;
using MobileWebApi.Data;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Responses;
using MobileWebApi.Repositories.Interfaces;
using MobileWebApi.Resources;

namespace MobileWebApi.Repositories
{
    /// <summary>
    /// Loads the currently authenticated user's profile and assigned work roles.
    /// </summary>
    public class MeRepository : IMeRepository
    {
        private readonly DapperContext _context;
        private readonly ITenantContext _tenantContext;
        private readonly QueryProvider _queries;
        private readonly ILogger<MeRepository> _logger;

        public MeRepository(
            DapperContext context,
            ITenantContext tenantContext,
            QueryProvider queries,
            ILogger<MeRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<MeResponse?> GetCurrentUserAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            var userId = _tenantContext.GetRequiredUserId();
            var tenantId = _tenantContext.GetRequiredOrganisationId();

            try
            {
                using var connection = _context.CreateConnection();

                var user = await connection.QueryFirstOrDefaultAsync<MeUserRow>(
                    _queries.Get("Me_GetCurrentUser"),
                    new { UserId = userId });

                if (user == null)
                {
                    _logger.LogWarning(
                        LogMessages.Me.UserNotFound,
                        userId,
                        tenantId);
                    return null;
                }

                var employeeId = await connection.QueryFirstOrDefaultAsync<int?>(
                    _queries.Get("GetEmployeeIdByUserId"),
                    new { UserId = userId });

                var assignedRoles = (await connection.QueryAsync<WorkRoleDto>(
                    _queries.Get("Me_GetActiveWorkRolesByUserId"),
                    new { UserId = userId })).ToList();

                var defaultUserRoleId = await connection.QueryFirstOrDefaultAsync<int?>(
                    _queries.Get("GetWorkRoleIdByName"),
                    new { WorkRoleName = WorkRoleHelper.DefaultWorkRoleName }) ?? 0;

                var workRoles = WorkRoleHelper.BuildMeWorkRoles(assignedRoles, defaultUserRoleId);
                var primaryWorkRole = WorkRoleHelper.ResolvePrimaryWorkRole(workRoles.Select(r => r.Name));

                stopwatch.Stop();
                _logger.LogInformation(
                    LogMessages.Me.ProfileLoaded,
                    userId,
                    tenantId,
                    primaryWorkRole,
                    stopwatch.ElapsedMilliseconds);

                return new MeResponse
                {
                    UserId = user.UserId,
                    TenantId = tenantId,
                    EmployeeId = employeeId > 0 ? employeeId : null,
                    Username = user.Username ?? string.Empty,
                    DisplayName = user.DisplayName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Mobile = user.MobileNumber ?? string.Empty,
                    WorkRoles = workRoles,
                    PrimaryWorkRole = primaryWorkRole
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogException(
                    ExceptionCodes.Me.GetCurrentUser,
                    nameof(GetCurrentUserAsync),
                    ex,
                    userId);
                throw;
            }
        }

        private sealed class MeUserRow
        {
            public int UserId { get; set; }
            public string? Username { get; set; }
            public string? DisplayName { get; set; }
            public string? Email { get; set; }
            public string? MobileNumber { get; set; }
        }
    }
}
