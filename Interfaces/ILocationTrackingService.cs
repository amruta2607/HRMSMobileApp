using MobileWebApi.Models;
using MobileWebApi.Models.Responses;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingService
    {
        Task<LocationTrackingResponse> RecordLocationAsync(LocationTrackingRequest request, int currentUserId, int organisationId);
        Task<LocationTrackingBatchResponse> RecordLocationBatchAsync(LocationTrackingBatchRequest request, int currentUserId, int organisationId);
        /// <summary>
        /// Resolves UserId → EmployeeId and returns today's location tracking path for that employee.
        /// </summary>
        Task<(bool Success, string Message, TodayLocationTrackingResponse? Data)> GetTodayPathAsync(
            int userId,
            int organisationId);
    }
}
