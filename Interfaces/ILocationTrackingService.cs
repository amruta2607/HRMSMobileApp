using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingService
    {
		Task<LocationTrackingResponse> RecordLocationAsync(LocationTrackingRequest request, int currentUserId, int organisationId);
		Task<LocationTrackingBatchResponse> RecordLocationBatchAsync(LocationTrackingBatchRequest request, int currentUserId, int organisationId);

		/// <summary>
		/// Returns today's location tracking path for the employee linked to <paramref name="userId"/>.
		/// </summary>
		Task<(bool Success, string Message, TodayLocationTrackingResponse? Data)> GetTodayPathAsync(
            int userId,
            int organisationId);

		/// <summary>
		/// Returns location tracking records for the employee linked to <paramref name="userId"/>
		/// on the specified <paramref name="date"/>.
		/// </summary>
		Task<(bool Success, string Message, ByDateLocationTrackingResponse? Data)> GetPathByDateAsync(
            int userId,
            DateTime date,
            int organisationId);
    }
}
