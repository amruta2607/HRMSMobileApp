using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Services
{
    public class LocationTrackingConfigurationService : ILocationTrackingConfigurationService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITenantConfigurationRepository _tenantConfigurationRepository;
        private readonly IMobileTenantConfigurationRepository _mobileTenantConfigurationRepository;
        private readonly ILocationTrackingConfigurationRepository _locationTrackingConfigurationRepository;
        private readonly ILogger<LocationTrackingConfigurationService> _logger;

        public LocationTrackingConfigurationService(
            IEmployeeRepository employeeRepository,
            ITenantConfigurationRepository tenantConfigurationRepository,
            IMobileTenantConfigurationRepository mobileTenantConfigurationRepository,
            ILocationTrackingConfigurationRepository locationTrackingConfigurationRepository,
            ILogger<LocationTrackingConfigurationService> logger)
        {
            _employeeRepository = employeeRepository;
            _tenantConfigurationRepository = tenantConfigurationRepository;
            _mobileTenantConfigurationRepository = mobileTenantConfigurationRepository;
            _locationTrackingConfigurationRepository = locationTrackingConfigurationRepository;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, LocationTrackingConfigurationResponse? Data)> GetConfigurationAsync(
            int userId,
            int organisationId)
        {
            var tenantConfig = await _tenantConfigurationRepository.GetTenantConfigurationRowByTenantIdAsync(organisationId);
            if (tenantConfig == null)
            {
                return (false, LocationTrackingConfigurationMessages.TenantNotFound, null);
            }

            var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
            if (employee == null)
            {
                return (false, LocationTrackingConfigurationMessages.EmployeeNotFound, null);
            }

            if (employee.OrganisationId != organisationId)
            {
                return (false, LocationTrackingConfigurationMessages.EmployeeDoesNotBelongToTenant, null);
            }

            var mobileTenantConfig = await _mobileTenantConfigurationRepository.GetByTenantIdAsync(organisationId);
            if (mobileTenantConfig == null)
            {
                return (false, LocationTrackingConfigurationMessages.TenantConfigurationNotFound, null);
            }

            if (!mobileTenantConfig.LocationTrackingConfigurationId.HasValue
                || mobileTenantConfig.LocationTrackingConfigurationId.Value <= 0)
            {
                return (false, LocationTrackingConfigurationMessages.LocationTrackingConfigurationNotFound, null);
            }

            var locationTrackingConfiguration = await _locationTrackingConfigurationRepository
                .GetByIdAsync(mobileTenantConfig.LocationTrackingConfigurationId.Value);

            if (locationTrackingConfiguration == null)
            {
                return (false, LocationTrackingConfigurationMessages.LocationTrackingConfigurationNotFound, null);
            }

            var settings = LocationTrackingSettingsHelper.Resolve(
                mobileTenantConfig.IsAttendanceEnabled,
                mobileTenantConfig.EnableLocationTracking,
                mobileTenantConfig.EnableEmployeeLevelLocationTracking,
                employee.EnableLocationTracking);

            _logger.LogInformation(
                LogMessages.LocationTrackingConfiguration.FetchingConfiguration,
                employee.Id,
                organisationId);

            var response = MapToResponse(settings, locationTrackingConfiguration);
            return (true, LocationTrackingConfigurationMessages.ConfigurationFetchedSuccessfully, response);
        }

        private static LocationTrackingConfigurationResponse MapToResponse(
            (bool AttendanceEnabled, bool EnableLocationTracking, bool EnableEmployeeLevelLocationTracking, bool EmployeeLocationTrackingEnabled) settings,
            LocationTrackingConfiguration configuration)
        {
            // Hierarchical master-switch resolution (mirrors the login API):
            // 1. EnableLocationTracking is the master switch. If it is off, all
            //    employee-level flags are forced false and employee settings ignored.
            // 2. When on, EnableEmployeeLevelLocationTracking gates whether the
            //    employee's own EnableLocationTracking value is surfaced.
            bool effectiveEnableEmployeeLevelLocationTracking;
            bool effectiveEmployeeLocationTrackingEnabled;
            if (!settings.EnableLocationTracking)
            {
                effectiveEnableEmployeeLevelLocationTracking = false;
                effectiveEmployeeLocationTrackingEnabled = false;
            }
            else if (!settings.EnableEmployeeLevelLocationTracking)
            {
                effectiveEnableEmployeeLevelLocationTracking = false;
                effectiveEmployeeLocationTrackingEnabled = false;
            }
            else
            {
                effectiveEnableEmployeeLevelLocationTracking = true;
                effectiveEmployeeLocationTrackingEnabled = settings.EmployeeLocationTrackingEnabled;
            }

            return new LocationTrackingConfigurationResponse
            {
                AttendanceEnabled = settings.AttendanceEnabled,
                EnableLocationTracking = settings.EnableLocationTracking,
                EnableEmployeeLevelLocationTracking = effectiveEnableEmployeeLevelLocationTracking,
                EmployeeLocationTrackingEnabled = effectiveEmployeeLocationTrackingEnabled,

                GPSPollingInterval = configuration.GPSPollingInterval,
                MinimumDisplacement = configuration.MinimumDisplacement,
                GPSAccuracyThreshold = configuration.GPSAccuracyThreshold,
                DuplicateLocationRadius = configuration.DuplicateLocationRadius,
                AutoPunchOutTimeout = configuration.AutoPunchOutTimeout,
                OfflineStorageLimit = configuration.OfflineStorageLimit,
                AutoDataCleanupDays = configuration.AutoDataCleanupDays,
                RetryInterval = configuration.RetryInterval,
                ServerSyncBatchSize = configuration.ServerSyncBatchSize,

                GeofenceRadius = configuration.GeofenceRadius,
                EnableFromAnywhere = configuration.EnableFromAnywhere,
                BlockPunchOnHoliday = configuration.BlockPunchOnHoliday,
                EnableLocationGapValidation = configuration.EnableLocationGapValidation,
                EnableBatteryOptimizationCheck = configuration.EnableBatteryOptimizationCheck,
                BatteryOptimizationMode = configuration.BatteryOptimizationMode,

                AutoPunchOutOnGPSTurnOff = configuration.AutoPunchOutOnGPSTurnOff,
                AutoPunchOutOnLocationServicesOff = configuration.AutoPunchOutOnLocationServicesOff,
                AutoPunchOutOnAppKilled = configuration.AutoPunchOutOnAppKilled,
                AutoPunchOutOnPowerSavingMode = configuration.AutoPunchOutOnPowerSavingMode,
                AutoPunchOutOnAirplaneMode = configuration.AutoPunchOutOnAirplaneMode,
                LocationTimeoutDuration = configuration.LocationTimeoutDuration,

                DuplicateSessionCheck = LocationTrackingFixedRules.DuplicateSessionCheck,
                AlwaysAllowPermissionCheck = LocationTrackingFixedRules.AlwaysAllowPermissionCheck,
                PermissionRevokedAutoPunchOut = LocationTrackingFixedRules.PermissionRevokedAutoPunchOut
            };
        }
    }
}
