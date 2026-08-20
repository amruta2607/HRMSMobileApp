using MobileWebApi.Models.Responses;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingService
    {
        /// <summary>
        /// Resolves UserId → EmployeeId and returns today's location tracking path for that employee.
        /// </summary>
        Task<(bool Success, string Message, TodayLocationTrackingResponse? Data)> GetTodayPathAsync(
            int userId,
            int organisationId);
    }
}
