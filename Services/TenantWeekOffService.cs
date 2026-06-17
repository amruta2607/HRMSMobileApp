using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Services
{
    public class TenantWeekOffService : ITenantWeekOffService
    {
        private readonly ITenantWeekOffRepository _repository;

        public TenantWeekOffService(ITenantWeekOffRepository repository)
        {
            _repository = repository;
        }

        public async Task<TenantWeekOffResponseDto?> GetTenantWeekOffDaysAsync(int tenantId)
        {
            var tenantConfigurationId = await _repository.GetTenantConfigurationIdByTenantIdAsync(tenantId);
            if (!tenantConfigurationId.HasValue)
            {
                return null;
            }

            var weekOffDays = await _repository.GetWeekOffDaysByTenantIdAsync(tenantId);

            return new TenantWeekOffResponseDto
            {
                TenantId = tenantId,
                TenantConfigurationId = tenantConfigurationId.Value,
                WeekOffDays = weekOffDays
            };
        }
    }
}
