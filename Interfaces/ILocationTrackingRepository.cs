using MobileWebApi.Models;
using MobileWebApi.Models.Responses;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingRepository
    {
        Task<int> InsertAsync(
            int employeeId,
            int tenantId,
            decimal latitude,
            decimal longitude,
            DateTime trackingDateTime,
            string? locationFrom,
            int insertUserId);

        Task<int> InsertBatchAsync(
            int employeeId,
            int tenantId,
            IReadOnlyList<LocationTrackingInsertRecord> records,
            int insertUserId);

        /// <summary>
        /// Returns today's LocationTracking rows for the employee/tenant, ordered chronologically.
        /// </summary>
        Task<IReadOnlyList<LocationTrackingPointRow>> GetTodayByEmployeeIdAsync(
            int employeeId,
            int tenantId,
            DateTime today);
    }
}
