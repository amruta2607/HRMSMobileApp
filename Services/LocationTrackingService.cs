using MobileWebApi.Constants;
using MobileWebApi.Helper;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Repositories;
using System.Globalization;

namespace MobileWebApi.Services
{
    public class LocationTrackingService : ILocationTrackingService
    {
        private readonly ILocationTrackingRepository _locationTrackingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<LocationTrackingService> _logger;

		private readonly IAttendanceRepository _attendanceRepository;
		private readonly IMobileTenantConfigurationRepository _mobileTenantConfigurationRepository;
		private readonly ITenantConfigurationRepository _tenantConfigurationRepository;
	

		public LocationTrackingService(
          ILocationTrackingRepository locationTrackingRepository,
            IUserRepository userRepository,
            IEmployeeRepository employeeRepository,
            ILogger<LocationTrackingService> logger, 
			IAttendanceRepository attendanceRepository,
			IMobileTenantConfigurationRepository mobileTenantConfigurationRepository,
			ITenantConfigurationRepository tenantConfigurationRepository)
        {
			_locationTrackingRepository = locationTrackingRepository;
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _logger = logger;
			_attendanceRepository = attendanceRepository;
			_mobileTenantConfigurationRepository = mobileTenantConfigurationRepository;
			_tenantConfigurationRepository = tenantConfigurationRepository;
        }

		public async Task<LocationTrackingResponse> RecordLocationAsync(
		   LocationTrackingRequest request,
		   int currentUserId,
		   int organisationId)
		{
			if (request.user_id <= 0)
			{
				return Failure(LocationTrackingMessages.UserIdRequired);
			}

			var itemError = ValidateLocationItem(
				request.latitude,
				request.longitude,
				request.timestamp);

			if (itemError != null)
			{
				return Failure(itemError);
			}

			var contextResult = await ValidateTrackingContextAsync(request.user_id, organisationId);
			if (contextResult.Error != null)
			{
				return Failure(contextResult.Error);
			}

			var employee = contextResult.Employee!;

			_logger.LogInformation(
				LogMessages.LocationTracking.RecordingLocation,
				employee.Id,
				organisationId);

			var trackingDateTimeIst = NormalizeToIndiaTime(request.timestamp);

			var recordId = await _locationTrackingRepository.InsertAsync(
				employee.Id,
				organisationId,
				RoundCoordinate(request.latitude!.Value),
				RoundCoordinate(request.longitude!.Value),
				trackingDateTimeIst,
				NormalizeLocationFrom(request.location_from),
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

		public async Task<LocationTrackingBatchResponse> RecordLocationBatchAsync(
			LocationTrackingBatchRequest request,
			int currentUserId,
			int organisationId)
		{
			if (request.user_id <= 0)
			{
				return BatchFailure(LocationTrackingMessages.UserIdRequired);
			}

			if (request.locations == null || request.locations.Count == 0)
			{
				return BatchFailure(LocationTrackingMessages.LocationsRequired);
			}

			var contextResult = await ValidateTrackingContextAsync(request.user_id, organisationId);
			if (contextResult.Error != null)
			{
				return BatchFailure(contextResult.Error);
			}

			var employee = contextResult.Employee!;
			var failedRecords = new List<LocationTrackingBatchFailedRecord>();
			var validRecords = new List<LocationTrackingInsertRecord>();

			foreach (var location in request.locations)
			{
				var itemError = ValidateLocationItem(
					location.latitude,
					location.longitude,
					location.timestamp);

				if (itemError != null)
				{
					failedRecords.Add(new LocationTrackingBatchFailedRecord
					{
						timestamp = location.timestamp == default ? null : location.timestamp,
						latitude = location.latitude,
						longitude = location.longitude,
						Reason = itemError
					});
					continue;
				}

				validRecords.Add(new LocationTrackingInsertRecord
				{
					Latitude = RoundCoordinate(location.latitude!.Value),
					Longitude = RoundCoordinate(location.longitude!.Value),
					TrackingDateTime = NormalizeToIndiaTime(location.timestamp),
					LocationFrom = NormalizeLocationFrom(location.location_from)
				});
			}

			var totalRecords = request.locations.Count;

			if (validRecords.Count == 0)
			{
				return new LocationTrackingBatchResponse
				{
					Success = false,
					Message = LocationTrackingMessages.BatchAllRecordsInvalid,
					TotalRecords = totalRecords,
					InsertedRecords = 0,
					FailedRecords = failedRecords.Count,
					FailedRecordDetails = failedRecords
				};
			}

			_logger.LogInformation(
				LogMessages.LocationTracking.ProcessingLocationBatch,
				employee.Id,
				organisationId,
				validRecords.Count);

			var insertedRecords = await _locationTrackingRepository.InsertBatchAsync(
				employee.Id,
				organisationId,
				validRecords,
				currentUserId);

			if (insertedRecords <= 0)
			{
				_logger.LogWarning(LogMessages.LocationTracking.FailedToRecordLocationBatch, employee.Id);
				return new LocationTrackingBatchResponse
				{
					Success = false,
					Message = LocationTrackingMessages.FailedToRecordLocationBatch,
					TotalRecords = totalRecords,
					InsertedRecords = 0,
					FailedRecords = totalRecords,
					FailedRecordDetails = failedRecords.Count > 0
						? failedRecords
						: request.locations.Select(l => new LocationTrackingBatchFailedRecord
						{
							timestamp = l.timestamp == default ? null : l.timestamp,
							latitude = l.latitude,
							longitude = l.longitude,
							Reason = LocationTrackingMessages.FailedToRecordLocation
						}).ToList()
				};
			}

			var failedCount = failedRecords.Count;
			return new LocationTrackingBatchResponse
			{
				Success = true,
				Message = failedCount == 0
					? LocationTrackingMessages.BatchProcessedSuccessfully
					: LocationTrackingMessages.BatchPartiallyProcessed,
				TotalRecords = totalRecords,
				InsertedRecords = insertedRecords,
				FailedRecords = failedCount,
				FailedRecordDetails = failedCount > 0 ? failedRecords : null
			};
		}

		private async Task<(Employee? Employee, string? Error)> ValidateTrackingContextAsync(
			int userId,
			int organisationId)
		{
			var tenantConfig = await _tenantConfigurationRepository.GetTenantConfigurationRowByTenantIdAsync(organisationId);
			if (tenantConfig == null)
			{
				return (null, LocationTrackingMessages.TenantNotFound);
			}

			var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
			if (employee == null)
			{
				return (null, LocationTrackingMessages.EmployeeNotFound);
			}

			if (employee.OrganisationId != organisationId)
			{
				return (null, LocationTrackingMessages.EmployeeDoesNotBelongToTenant);
			}

			var openPunch = await _attendanceRepository.GetOpenPunchByEmployeeId(employee.Id);
			if (openPunch == null || !openPunch.PunchIn.HasValue)
			{
				return (null, LocationTrackingMessages.EmployeeNotPunchedIn);
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
				return (null, LocationTrackingMessages.LocationTrackingDisabled);
			}

			return (employee, null);
		}

		private static string? ValidateLocationItem(double? latitude, double? longitude, DateTime trackingDateTime)
		{
			if (!latitude.HasValue)
			{
				return LocationTrackingMessages.LatitudeRequired;
			}

			if (!longitude.HasValue)
			{
				return LocationTrackingMessages.LongitudeRequired;
			}

			if (latitude.Value < -90 || latitude.Value > 90)
			{
				return LocationTrackingMessages.InvalidLatitude;
			}

			if (longitude.Value < -180 || longitude.Value > 180)
			{
				return LocationTrackingMessages.InvalidLongitude;
			}

			if (trackingDateTime == default)
			{
				return LocationTrackingMessages.TrackingDateTimeRequired;
			}

			return null;
		}

		private static LocationTrackingResponse Failure(string message) =>
			new() { Success = false, Message = message };

		private static LocationTrackingBatchResponse BatchFailure(string message) =>
			new() { Success = false, Message = message };

		private static decimal RoundCoordinate(double value) =>
			Math.Round(Convert.ToDecimal(value), 6, MidpointRounding.AwayFromZero);

		private static string? NormalizeLocationFrom(string? locationFrom) =>
			string.IsNullOrWhiteSpace(locationFrom) ? null : locationFrom.Trim();

		private static DateTime NormalizeToIndiaTime(DateTime trackingDateTime)
		{
			// Mobile typically sends local (India) time like "2026-07-07T05:25:37" (Kind = Unspecified).
			// If a UTC timestamp is sent (e.g., "...Z"), convert it to India Standard Time before storing.
			try
			{
				var indiaTz = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

				return trackingDateTime.Kind switch
				{
					DateTimeKind.Utc => TimeZoneInfo.ConvertTimeFromUtc(trackingDateTime, indiaTz),
					DateTimeKind.Local => TimeZoneInfo.ConvertTime(trackingDateTime, TimeZoneInfo.Local, indiaTz),
					_ => trackingDateTime // Unspecified: assume already IST
				};
			}
			catch
			{
				// If timezone lookup fails for any reason, store the timestamp as provided.
				return trackingDateTime;
			}
		}
    
        public async Task<(bool Success, string Message, TodayLocationTrackingResponse? Data)> GetTodayPathAsync(
            int userId,
            int organisationId)
        {
            if (userId <= 0)
            {
                return (false, LocationTrackingMessages.UserIdRequired, null);
            }

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(LogMessages.LocationTracking.UserNotFound, userId);
                return (false, LocationTrackingMessages.UserNotFound, null);
            }

            // Tenant safety: requested user must belong to the caller's organisation.
            if (user.OrganisationId != organisationId)
            {
                _logger.LogWarning(
                    "User organisation mismatch while fetching today's location path. UserId={UserId}, UserOrg={UserOrg}, RequestOrg={RequestOrg}",
                    userId,
                    user.OrganisationId,
                    organisationId);
                return (false, LocationTrackingMessages.EmployeeDoesNotBelongToTenant, null);
            }

            var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
            if (employee == null || employee.Id <= 0)
            {
                _logger.LogWarning(LogMessages.LocationTracking.EmployeeNotFoundForUser, userId);
                return (false, LocationTrackingMessages.EmployeeNotAssociatedWithUser, null);
            }

            if (employee.OrganisationId != organisationId)
            {
                return (false, LocationTrackingMessages.EmployeeDoesNotBelongToTenant, null);
            }

            var today = DateTime.Now.Date;

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
                .Select(MapToPointDto)
                .ToList();

            _logger.LogInformation(
                LogMessages.LocationTracking.TodayPathFetched,
                points.Count,
                employee.Id,
                organisationId,
                today);

            var response = new TodayLocationTrackingResponse
            {
                EmployeeId = employee.Id,
                Date = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Points = points
            };

            return (true, LocationTrackingMessages.TodayPathFetchedSuccessfully, response);
        }

        private static TodayLocationTrackingPointDto MapToPointDto(LocationTrackingPointRow row)
        {
            var pointDate = row.Date.Date;
            var pointTime = row.Time;

            return new TodayLocationTrackingPointDto
            {
                Id = row.Id,
                Latitude = row.Latitude,
                Longitude = row.Longitude,
                Date = pointDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                Time = pointTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
                LocationFrom = row.LocationFrom
            };
        }
    }
}
