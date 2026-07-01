using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Services
{
    public class LocationTrackingService : ILocationTrackingService
    {
        private readonly ILocationTrackingRepository _locationTrackingRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMobileTenantConfigurationRepository _mobileTenantConfigurationRepository;
        private readonly ITenantConfigurationRepository _tenantConfigurationRepository;
        private readonly ILogger<LocationTrackingService> _logger;

        public LocationTrackingService(
            ILocationTrackingRepository locationTrackingRepository,
            IEmployeeRepository employeeRepository,
            IAttendanceRepository attendanceRepository,
            IMobileTenantConfigurationRepository mobileTenantConfigurationRepository,
            ITenantConfigurationRepository tenantConfigurationRepository,
            ILogger<LocationTrackingService> logger)
        {
            _locationTrackingRepository = locationTrackingRepository;
            _employeeRepository = employeeRepository;
            _attendanceRepository = attendanceRepository;
            _mobileTenantConfigurationRepository = mobileTenantConfigurationRepository;
            _tenantConfigurationRepository = tenantConfigurationRepository;
            _logger = logger;
        }

        public async Task<LocationTrackingResponse> RecordLocationAsync(
            LocationTrackingRequest request,
            int currentUserId,
            int organisationId)
        {
            if (request.userId <= 0)
            {
                return Failure(LocationTrackingMessages.UserIdRequired);
            }

            if (!request.latitude.HasValue)
            {
                return Failure(LocationTrackingMessages.LatitudeRequired);
            }

            if (!request.longitude.HasValue)
            {
                return Failure(LocationTrackingMessages.LongitudeRequired);
            }

            if (request.latitude.Value < -90 || request.latitude.Value > 90)
            {
                return Failure(LocationTrackingMessages.InvalidLatitude);
            }

            if (request.longitude.Value < -180 || request.longitude.Value > 180)
            {
                return Failure(LocationTrackingMessages.InvalidLongitude);
            }

            if (request.trackingDateTime == default)
            {
                return Failure(LocationTrackingMessages.TrackingDateTimeRequired);
            }

            var tenantConfig = await _tenantConfigurationRepository.GetTenantConfigurationRowByTenantIdAsync(organisationId);
            if (tenantConfig == null)
            {
                return Failure(LocationTrackingMessages.TenantNotFound);
            }

            var employee = await _employeeRepository.GetEmployeebyUserIdAsync(request.userId);
            if (employee == null)
            {
                return Failure(LocationTrackingMessages.EmployeeNotFound);
            }

            if (employee.OrganisationId != organisationId)
            {
                return Failure(LocationTrackingMessages.EmployeeDoesNotBelongToTenant);
            }

            var openPunch = await _attendanceRepository.GetOpenPunchByEmployeeId(employee.Id);
            if (openPunch == null || !openPunch.PunchIn.HasValue)
            {
                return Failure(LocationTrackingMessages.EmployeeNotPunchedIn);
            }

            var mobileTenantConfig = await _mobileTenantConfigurationRepository.GetByTenantIdAsync(organisationId);
            var attendanceEnabled = mobileTenantConfig?.IsAttendanceEnabled ?? false;
            var tenantLocationTrackingEnabled = mobileTenantConfig?.EnableLocationTracking ?? false;
            var enableEmployeeLevelLocationTracking = mobileTenantConfig?.EnableEmployeeLevelLocationTracking ?? false;

            if (!LocationTrackingSettingsHelper.ShouldTrackLocation(
                attendanceEnabled,
                tenantLocationTrackingEnabled,
                enableEmployeeLevelLocationTracking,
                employee.EnableLocationTracking))
            {
                return Failure(LocationTrackingMessages.LocationTrackingDisabled);
            }

            _logger.LogInformation(
                LogMessages.LocationTracking.RecordingLocation,
                employee.Id,
                organisationId);

            var trackingDateTime = request.trackingDateTime;
            var recordId = await _locationTrackingRepository.InsertAsync(
                employee.Id,
                organisationId,
                RoundCoordinate(request.latitude.Value),
                RoundCoordinate(request.longitude.Value),
                trackingDateTime,
                currentUserId);

            if (recordId <= 0)
            {
                _logger.LogWarning(LogMessages.LocationTracking.FailedToRecordLocation, employee.Id);
                return Failure(LocationTrackingMessages.FailedToRecordLocation);
            }

            return new LocationTrackingResponse
            {
                Success = true,
                Message = LocationTrackingMessages.LocationRecordedSuccessfully
            };
        }

        private static LocationTrackingResponse Failure(string message) =>
            new() { Success = false, Message = message };

        private static decimal RoundCoordinate(double value) =>
            Math.Round(Convert.ToDecimal(value), 6, MidpointRounding.AwayFromZero);
    }
}
