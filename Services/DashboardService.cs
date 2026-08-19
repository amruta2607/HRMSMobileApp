using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IReadOnlyList<DashboardBirthdayDto>> GetUpcomingBirthdaysAsync(int tenantId)
        {
            try
            {
                var (today, endDate) = GetDashboardWindow();
                return await _dashboardRepository.GetUpcomingBirthdaysAsync(tenantId, today, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Dashboard.GetBirthdays, nameof(GetUpcomingBirthdaysAsync), ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<DashboardWorkAnniversaryDto>> GetUpcomingWorkAnniversariesAsync(int tenantId)
        {
            try
            {
                var (today, endDate) = GetDashboardWindow();
                return await _dashboardRepository.GetUpcomingWorkAnniversariesAsync(tenantId, today, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Dashboard.GetWorkAnniversaries, nameof(GetUpcomingWorkAnniversariesAsync), ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<DashboardAwardDto>> GetUpcomingAwardsAsync(int tenantId)
        {
            try
            {
                var (today, endDate) = GetDashboardWindow();
                return await _dashboardRepository.GetUpcomingAwardsAsync(tenantId, today, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Dashboard.GetAwards, nameof(GetUpcomingAwardsAsync), ex);
                throw;
            }
        }

        private static (DateTime Today, DateTime EndDate) GetDashboardWindow()
        {
            var today = DateTime.Today;
            return (today, today.AddDays(4));
        }
    }
}
