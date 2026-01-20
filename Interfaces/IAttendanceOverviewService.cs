using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IAttendanceOverviewService
    {
        Task<AttendanceOverviewResponse> GetAttendanceOverviewAsync(AttendanceOverviewRequest request);
    }
}

