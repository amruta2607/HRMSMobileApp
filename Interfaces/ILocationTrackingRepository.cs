using MobileWebApi.Models;

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
        /// Returns today's location tracking points for an employee within a tenant,
        /// ordered chronologically from first to last.
        /// </summary>
        Task<IReadOnlyList<LocationTrackingPointRow>> GetTodayByEmployeeIdAsync(
            int employeeId,
            int tenantId,
            DateTime today);
    }
}
