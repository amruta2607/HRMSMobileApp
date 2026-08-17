using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IMobileDashboardService
    {
        Task<IReadOnlyList<TrainingDto>> GetLatestTrainingsAsync(int tenantId);
        Task<IReadOnlyList<AnnouncementDto>> GetLatestAnnouncementsAsync(int tenantId);
        Task<IReadOnlyList<EventDto>> GetLatestEventsAsync(int tenantId);
        Task<IReadOnlyList<HolidayDto>> GetLatestHolidaysAsync(int tenantId);
        Task<IReadOnlyList<BirthdayDto>> GetBirthdaysAsync(int tenantId);
        Task<IReadOnlyList<WorkAnniversaryDto>> GetWorkAnniversariesAsync(int tenantId);
        Task<IReadOnlyList<AwardDto>> GetAwardsAsync(int tenantId);
    }
}

