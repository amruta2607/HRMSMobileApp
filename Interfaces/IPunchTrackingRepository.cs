using System.Data;
using MobileWebApi.Models;
using MobileWebApi.Models.Responses;

namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Repository for punch tracking records.
    /// </summary>
    public interface IPunchTrackingRepository
    {
        /// <summary>
        /// Inserts a punch tracking record within its own transaction.
        /// </summary>
        Task<int> InsertPunchTrackingAsync(PunchTracking tracking);

        /// <summary>
        /// Inserts a punch tracking record within an existing transaction.
        /// </summary>
        Task<int> InsertPunchTrackingAsync(PunchTracking tracking, IDbConnection connection, IDbTransaction transaction);

        /// <summary>
        /// Gets the most recent punch tracking record for an employee on a given date.
        /// </summary>
        Task<PunchTracking?> GetLastPunchTrackingAsync(int employeeId, int tenantId, DateTime punchDate);

        /// <summary>
        /// Gets the punch tracking timeline for the authenticated employee.
        /// Resolves EmployeeId and TenantId from JWT claims.
        /// </summary>
        Task<PunchTrackingTimelineResult> GetPunchTrackingTimelineAsync(int punchId);
    }
}
