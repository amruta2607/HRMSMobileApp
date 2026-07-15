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
    }
}
