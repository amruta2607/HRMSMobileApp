using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Services
{
    public class MobileModuleAccessService : IMobileModuleAccessService
    {
        private readonly IMobileTenantConfigurationRepository _repo;
        private readonly ILogger<MobileModuleAccessService> _logger;

        public MobileModuleAccessService(
            IMobileTenantConfigurationRepository repo,
            ILogger<MobileModuleAccessService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<MobileAccessDto> GetModuleAccess(int organizationId)
        {
            if (organizationId <= 0)
                return new MobileAccessDto();

            var cfg = await _repo.GetByTenantIdAsync(organizationId);
            if (cfg == null)
                return new MobileAccessDto();

            if (!cfg.IsEnableMobile)
            {
                return new MobileAccessDto
                {
                    IsEnableMobile = false,
                    Attendance = false,
                    Leave = false,
                    Payroll = false
                };
            }

            return new MobileAccessDto
            {
                IsEnableMobile = true,
                Attendance = cfg.IsAttendanceEnabled,
                Leave = cfg.IsLeaveEnabled,
                Payroll = cfg.IsPayrollEnabled
            };
        }

        public async Task<bool> HasAccess(int tenantId, string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
                return false;

            var access = await GetModuleAccess(tenantId);
            if (!access.IsEnableMobile)
                return false;

            return moduleName.Trim() switch
            {
                "Attendance" => access.Attendance,
                "Leave" => access.Leave,
                "Payroll" => access.Payroll,
                _ => false
            };
        }
    }
}

