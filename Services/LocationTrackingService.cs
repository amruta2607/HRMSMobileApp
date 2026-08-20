using MobileWebApi.Constants;
using MobileWebApi.Interfaces;
using MobileWebApi.Models.Responses;

namespace MobileWebApi.Services
{
    public class LocationTrackingService : ILocationTrackingService
    {
        private readonly ILocationTrackingRepository _locationTrackingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<LocationTrackingService> _logger;

        public LocationTrackingService(
            ILocationTrackingRepository locationTrackingRepository,
            IUserRepository userRepository,
            IEmployeeRepository employeeRepository,
            ILogger<LocationTrackingService> logger)
        {
            _locationTrackingRepository = locationTrackingRepository;
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, TodayLocationTrackingResponse? Data)> GetTodayPathAsync(
            int userId,
            int organisationId)
        {
            if (userId <= 0)
            {
                return (false, LocationTrackingMessages.InvalidUserId, null);
            }

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return (false, LocationTrackingMessages.UserNotFound, null);
            }

            // LocationTracking stores EmployeeId; map via Employee.SystemUserId = Users.UserId.
            var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
            if (employee == null || employee.Id <= 0)
            {
                return (false, LocationTrackingMessages.EmployeeNotFoundForUser, null);
            }

            if (employee.OrganisationId != organisationId)
            {
                return (false, LocationTrackingMessages.EmployeeDoesNotBelongToTenant, null);
            }

            var today = DateTime.Today;

            _logger.LogInformation(
                LogMessages.LocationTracking.FetchingTodayPath,
                userId,
                employee.Id,
                organisationId,
                today);

            var rows = await _locationTrackingRepository.GetTodayByEmployeeIdAsync(
                employee.Id,
                organisationId,
                today);

            var points = rows
                .Select(row => new LocationTrackingPointDto
                {
                    Id = row.Id,
                    Latitude = row.Latitude,
                    Longitude = row.Longitude,
                    Date = row.Date,
                    Time = row.Time,
                    LocationFrom = row.LocationFrom
                })
                .ToList();

            _logger.LogInformation(
                LogMessages.LocationTracking.TodayPathFetched,
                points.Count,
                employee.Id,
                today);

            return (
                true,
                LocationTrackingMessages.TodayPathFetchedSuccessfully,
                new TodayLocationTrackingResponse
                {
                    EmployeeId = employee.Id,
                    Date = today.ToString("yyyy-MM-dd"),
                    Points = points
                });
        }
    }
}
