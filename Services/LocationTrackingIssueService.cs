using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;

namespace MobileWebApi.Services
{
    public class LocationTrackingIssueService : ILocationTrackingIssueService
    {
        private readonly ILocationTrackingIssueRepository _issueRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITenantConfigurationRepository _tenantConfigurationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAlertRepository _alertRepository;
        private readonly ILogger<LocationTrackingIssueService> _logger;

        public LocationTrackingIssueService(
            ILocationTrackingIssueRepository issueRepository,
            IEmployeeRepository employeeRepository,
            ITenantConfigurationRepository tenantConfigurationRepository,
            IUserRepository userRepository,
            IAlertRepository alertRepository,
            ILogger<LocationTrackingIssueService> logger)
        {
            _issueRepository = issueRepository;
            _employeeRepository = employeeRepository;
            _tenantConfigurationRepository = tenantConfigurationRepository;
            _userRepository = userRepository;
            _alertRepository = alertRepository;
            _logger = logger;
        }

        public async Task<LocationTrackingResponse> AddLocationTrackingIssueAsync(
            LocationTrackingIssueRequest request,
            int currentUserId,
            int organisationId)
        {
            var validationError = ValidateRequest(request);
            if (validationError != null)
            {
                _logger.LogWarning(
                    LogMessages.LocationTrackingIssue.ValidationFailed,
                    currentUserId,
                    validationError);
                return Failure(validationError);
            }

            var tenantConfig = await _tenantConfigurationRepository.GetTenantConfigurationRowByTenantIdAsync(organisationId);
            if (tenantConfig == null)
            {
                _logger.LogWarning(
                    LogMessages.LocationTrackingIssue.TenantNotFound,
                    organisationId,
                    currentUserId);
                return Failure(LocationTrackingIssueMessages.TenantNotFound);
            }

            var employee = await _employeeRepository.GetEmployeebyUserIdAsync(request.user_id);
            if (employee == null)
            {
                _logger.LogWarning(
                    LogMessages.LocationTrackingIssue.EmployeeNotFound,
                    request.user_id,
                    currentUserId);
                return Failure(LocationTrackingIssueMessages.EmployeeNotFound);
            }

            if (employee.OrganisationId != organisationId)
            {
                _logger.LogWarning(
                    LogMessages.LocationTrackingIssue.EmployeeTenantMismatch,
                    request.user_id,
                    organisationId,
                    currentUserId);
                return Failure(LocationTrackingIssueMessages.EmployeeDoesNotBelongToTenant);
            }

            var now = DateTime.Now;
            var issue = new LocationTrackingIssue
            {
                UserId = request.user_id,
                TenantId = organisationId,
                IssueType = LocationTrackingIssueTypes.Normalize(request.issue_type),
                IssueDescription = request.issue_description.Trim(),
                Timestamp = request.timestamp,
                LastKnownLatitude = RoundCoordinate(request.last_known_latitude),
                LastKnownLongitude = RoundCoordinate(request.last_known_longitude),
                DeviceId = string.IsNullOrWhiteSpace(request.device_id) ? null : request.device_id.Trim(),
                InsertUserId = currentUserId,
                InsertDate = now,
                UpdateUserId = currentUserId,
                UpdateDate = now
            };

            _logger.LogInformation(
                LogMessages.LocationTrackingIssue.LoggingIssue,
                employee.Id,
                organisationId,
                issue.IssueType,
                currentUserId);

            int issueId;
            try
            {
                issueId = await _issueRepository.InsertAsync(issue);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    LogMessages.LocationTrackingIssue.FailedToInsert,
                    employee.Id,
                    organisationId);
                return Failure(LocationTrackingIssueMessages.FailedToLogIssue);
            }

            if (issueId <= 0)
            {
                _logger.LogWarning(
                    LogMessages.LocationTrackingIssue.FailedToInsert,
                    employee.Id,
                    organisationId);
                return Failure(LocationTrackingIssueMessages.FailedToLogIssue);
            }

            _logger.LogInformation(
                LogMessages.LocationTrackingIssue.IssueLoggedSuccessfully,
                issueId,
                employee.Id,
                organisationId,
                currentUserId);

            await TrySendAdminNotificationsAsync(employee, issue, organisationId, currentUserId);

            return new LocationTrackingResponse
            {
                Success = true,
                Message = LocationTrackingIssueMessages.IssueLoggedSuccessfully
            };
        }

        private static string? ValidateRequest(LocationTrackingIssueRequest request)
        {
            if (request.user_id <= 0)
            {
                return LocationTrackingMessages.UserIdRequired;
            }

            if (string.IsNullOrWhiteSpace(request.issue_type))
            {
                return LocationTrackingIssueMessages.IssueTypeRequired;
            }

            if (!LocationTrackingIssueTypes.IsValid(request.issue_type))
            {
                return LocationTrackingIssueMessages.InvalidIssueType;
            }

            if (string.IsNullOrWhiteSpace(request.issue_description))
            {
                return LocationTrackingIssueMessages.IssueDescriptionRequired;
            }

            if (request.timestamp == default)
            {
                return LocationTrackingIssueMessages.TimestampRequired;
            }

            if (request.last_known_latitude < -90 || request.last_known_latitude > 90)
            {
                return LocationTrackingMessages.InvalidLatitude;
            }

            if (request.last_known_longitude < -180 || request.last_known_longitude > 180)
            {
                return LocationTrackingMessages.InvalidLongitude;
            }

            return null;
        }

        private async Task TrySendAdminNotificationsAsync(
            Employee employee,
            LocationTrackingIssue issue,
            int organisationId,
            int currentUserId)
        {
            try
            {
                var users = await _userRepository.GetAllAsync(organisationId);
                var adminUsers = users
                    .Where(u => u.IsActive && (u.IsHrUser || u.IsTenantAdmin))
                    .ToList();

                if (adminUsers.Count == 0)
                {
                    _logger.LogInformation(
                        LogMessages.LocationTrackingIssue.NoAdminRecipients,
                        organisationId);
                    return;
                }

                var employeeName = GetEmployeeDisplayName(employee);
                var issueDisplayName = LocationTrackingIssueTypes.GetDisplayName(issue.IssueType);
                var title = LocationTrackingIssueMessages.ViolationNotificationTitle;
                var message = string.Format(
                    LocationTrackingIssueMessages.ViolationNotificationMessage,
                    employeeName,
                    issueDisplayName);

                foreach (var adminUser in adminUsers)
                {
                    await _alertRepository.CreateAlertAsync(new CreateAlertRequest
                    {
                        organization = organisationId,
                        UserId = adminUser.UserId,
                        EventId = null,
                        Title = title,
                        Message = message,
                        Status = NotificationStatusConstants.Unread,
                        InsertUserId = currentUserId
                    });
                }

                _logger.LogInformation(
                    LogMessages.LocationTrackingIssue.AdminNotificationsSent,
                    adminUsers.Count,
                    employee.Id,
                    organisationId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    LogMessages.LocationTrackingIssue.AdminNotificationFailed,
                    employee.Id,
                    organisationId);
            }
        }

        private static string GetEmployeeDisplayName(Employee employee)
        {
            if (!string.IsNullOrWhiteSpace(employee.Name))
            {
                return employee.Name.Trim();
            }

            var parts = new[] { employee.FirstName, employee.MiddleName, employee.LastName }
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part.Trim());

            var displayName = string.Join(" ", parts);
            return string.IsNullOrWhiteSpace(displayName) ? LocationTrackingIssueMessages.UnknownEmployeeName : displayName;
        }

        private static LocationTrackingResponse Failure(string message) =>
            new() { Success = false, Message = message };

        private static decimal RoundCoordinate(decimal value) =>
            Math.Round(value, 6, MidpointRounding.AwayFromZero);
    }
}
