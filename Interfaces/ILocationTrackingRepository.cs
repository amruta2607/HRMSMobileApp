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
            int insertUserId);
    }
}
