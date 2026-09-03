using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using MobileWebApi.Helper;
using System.Globalization;
using System.Text.Json;

namespace MobileWebApi.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _repo;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IEmployeeService _employeeService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly BlobService _blobService;
        private readonly ITenantWeekOffRepository _tenantWeekOffRepository;
        private readonly ILogger<AttendanceService> _logger;

        public AttendanceService(
            IAttendanceRepository repo, 
            IEmployeeRepository employeeRepository, 
            IEmployeeService employeeService,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            BlobService blobService,
            ITenantWeekOffRepository tenantWeekOffRepository,
            ILogger<AttendanceService> logger)
        {
            _repo = repo;
            _employeeRepository = employeeRepository;
            _employeeService = employeeService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _blobService = blobService;
            _tenantWeekOffRepository = tenantWeekOffRepository;
            _logger = logger;
        }

        /// <summary>
        /// Ensures PunchId is populated from Punch.Id when SQL only mapped Id.
        /// </summary>
        private static void EnsurePunchIds(IEnumerable<AttendanceReport> records)
        {
            foreach (var record in records)
            {
                if (!record.PunchId.HasValue && record.Id > 0)
                    record.PunchId = record.Id;
            }
        }

        /// <summary>
        /// Converts a stored private blob URL into a temporary read-only SAS URL for API clients.
        /// </summary>
        private string? ToClientPunchImageUrl(string? storedUrl)
            => _blobService.GenerateReadSasUrl(storedUrl);

        private void ApplyImageSasUrls(AttendanceReport report)
        {
            report.PunchInImage = ToClientPunchImageUrl(report.PunchInImage);
            report.PunchOutImage = ToClientPunchImageUrl(report.PunchOutImage);
        }

        private void ApplyImageSasUrls(IEnumerable<AttendanceReport> reports)
        {
            foreach (var report in reports)
                ApplyImageSasUrls(report);
        }

        private void ApplyImageSasUrls(IEnumerable<RealTimeAttendanceStatus> records)
        {
            foreach (var record in records)
            {
                record.PunchInImage = ToClientPunchImageUrl(record.PunchInImage);
                record.PunchOutImage = ToClientPunchImageUrl(record.PunchOutImage);
            }
        }

        private void ApplyImageSasUrls(IEnumerable<TodayPunchLogItem> records)
        {
            foreach (var record in records)
            {
                record.PunchInImage = ToClientPunchImageUrl(record.PunchInImage);
                record.PunchOutImage = ToClientPunchImageUrl(record.PunchOutImage);
            }
        }

        private const string MobileSource = "Mobile";

        /// <summary>
        /// Resolves EmployeeId from UserId by joining Users and Employee tables
        /// </summary>
        private async Task<int?> ResolveEmployeeIdFromUserIdAsync(int userId)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
                if (employee == null)
                {
                    _logger.LogWarning(LogMessages.EmployeeResolution.NoEmployeeFoundForUserId, userId);
                    return null;
                }
                return employee.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.EmployeeResolution.ErrorResolvingEmployeeIdFromUserId, userId);
                return null;
            }
        }

		public async Task<string> PunchInAsync(PunchInRequest req)
		{
			try
			{
				_logger.LogInformation(
					"PunchInAsync entry - punch_in_time: {PunchInTime} (Kind: {PunchInKind}), attendance_date: {AttendanceDate} (Kind: {AttendanceDateKind})",
					req.punch_in_time,
					req.punch_in_time.Kind,
					req.attendance_date,
					req.attendance_date.Kind);

				var employeeId = await ResolveEmployeeIdFromUserIdAsync(req.userId);

				if (!employeeId.HasValue)
				{
					_logger.LogWarning(
						LogMessages.EmployeeResolution.NoEmployeeFoundForUserId,
						req.userId);

					return "No employee found for the specified UserId.";
				}

				_logger.LogInformation(
					LogMessages.Attendance.ProcessingPunchIn,
					employeeId.Value);

				var punchIn = PreserveReceivedDateTime(req.punch_in_time);
				var attendanceDate = PreserveReceivedDateTime(req.attendance_date).Date;

				_logger.LogInformation(
					"PunchInAsync before save - EmployeeId: {EmployeeId}, PunchIn: {PunchIn} (Kind: {PunchInKind}), PunchDate: {PunchDate}",
					employeeId.Value,
					punchIn,
					punchIn.Kind,
					attendanceDate);

				// Prevent duplicate punch in
				var existingPunch = await _repo.GetPunchByEmployeeAndDate(
					employeeId.Value,
					attendanceDate);

				if (existingPunch != null && existingPunch.PunchIn != null)
				{
					_logger.LogWarning(AttendanceMessages.PunchInAlreadyDone);

					return AttendanceMessages.PunchInAlreadyDone;
				}

				// Upload Punch In image to Azure Blob, then persist blob URL only in Punch.PunchInImage
				string? punchInImage = null;
				if (req.PunchInImage != null && req.PunchInImage.Length > 0)
				{
					punchInImage = await _blobService.UploadAsync(req.PunchInImage, employeeId.Value);
					_logger.LogInformation("Punch-in image uploaded for employee {EmployeeId}", employeeId.Value);
				}

				// Location
				var coordinateIn = BuildCoordinate(
					req.latitude,
					req.longitude);

				var linkIn = GenerateGoogleMapLink(
					req.latitude,
					req.longitude);

				_ = await TryGetReverseGeocodedAddressAsync(
					req.latitude,
					req.longitude);

				// Insert attendance
				var punchId = await _repo.InsertPunchIn(
					employeeId.Value,
					punchIn,
					attendanceDate,
					MobileSource,
					coordinateIn,
					linkIn,
					punchInImage
				);

				if (punchId > 0)
				{
					var savedPunch = await _repo.GetPunchByIdAsync(punchId, await GetEmployeeTenantIdAsync(employeeId.Value));
					_logger.LogInformation(
						"PunchInAsync after save - PunchId: {PunchId}, Stored PunchIn: {StoredPunchIn}, Stored PunchDate: {StoredPunchDate}",
						punchId,
						savedPunch?.PunchIn,
						savedPunch?.PunchDate);

					_logger.LogInformation(
						LogMessages.Attendance.PunchInSuccessful,
						employeeId.Value);

					return AttendanceMessages.PunchInSuccessful;
				}

				_logger.LogWarning(AttendanceMessages.PunchInFailed);

				return AttendanceMessages.PunchInFailed;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while processing punch in");

				return "Error while processing punch in.";
			}
		}

		/// <summary>
		/// Punch Out
		/// Supports image upload + geo location
		/// Supports overnight shifts
		/// </summary>
		public async Task<string> PunchOutAsync(PunchOutRequest req)
		{
			try
			{
				_logger.LogInformation(
					"PunchOutAsync entry - punch_out_time: {PunchOutTime} (Kind: {PunchOutKind})",
					req.punch_out_time,
					req.punch_out_time.Kind);

				var employeeId = await ResolveEmployeeIdFromUserIdAsync(req.userId);

				if (!employeeId.HasValue)
				{
					_logger.LogWarning(
						LogMessages.EmployeeResolution.NoEmployeeFoundForUserId,
						req.userId);

					return "No employee found for the specified UserId.";
				}

				_logger.LogInformation(
					LogMessages.Attendance.ProcessingPunchOut,
					employeeId.Value);

				var punchOut = PreserveReceivedDateTime(req.punch_out_time);

				_logger.LogInformation(
					"PunchOutAsync before save - EmployeeId: {EmployeeId}, PunchOut: {PunchOut} (Kind: {PunchOutKind})",
					employeeId.Value,
					punchOut,
					punchOut.Kind);

				// Get open attendance
				var punch = await _repo.GetOpenPunchByEmployeeId(
					employeeId.Value);

				// Cross-day support
				if (punch == null || punch.PunchIn == null)
				{
					_logger.LogWarning(
						AttendanceMessages.CannotPunchOutWithoutPunchIn);

					return AttendanceMessages.CannotPunchOutWithoutPunchIn;
				}

				// Prevent duplicate punch out
				if (punch.PunchOut != null)
				{
					_logger.LogWarning(
						AttendanceMessages.PunchOutAlreadyDone);

					return AttendanceMessages.PunchOutAlreadyDone;
				}

				// Duration
				double? duration = CalculateDurationInMinutes(
					punch.PunchIn,
					punchOut);

				// Upload Punch Out image to Azure Blob, then persist blob URL only in Punch.PunchOutImage
				string? punchOutImage = null;
				if (req.PunchOutImage != null && req.PunchOutImage.Length > 0)
				{
					punchOutImage = await _blobService.UploadAsync(req.PunchOutImage, employeeId.Value);
					_logger.LogInformation("Punch-out image uploaded for employee {EmployeeId}", employeeId.Value);
				}

				// Location
				var coordinateOut = BuildCoordinate(
					req.latitude,
					req.longitude);

				var linkOut = GenerateGoogleMapLink(
					req.latitude,
					req.longitude);

				_ = await TryGetReverseGeocodedAddressAsync(
					req.latitude,
					req.longitude);

				// Update punch out — does not modify PunchInImage
				await _repo.UpdatePunchOut(
					punch.Id,
					punchOut,
					duration,
					MobileSource,
					coordinateOut,
					linkOut,
					punchOutImage,
					req.userId,
					req.punchOutReason
				);

				var savedPunch = await _repo.GetPunchByIdAsync(punch.Id, await GetEmployeeTenantIdAsync(employeeId.Value));
				_logger.LogInformation(
					"PunchOutAsync after save - PunchId: {PunchId}, Stored PunchOut: {StoredPunchOut}, Stored Duration: {StoredDuration}",
					punch.Id,
					savedPunch?.PunchOut,
					savedPunch?.Duration);

				_logger.LogInformation(
					LogMessages.Attendance.PunchOutSuccessful,
					employeeId.Value);

				return AttendanceMessages.PunchOutSuccessful;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error while processing punch out");

				return "Error while processing punch out.";
			}
		}

		public async Task<string> PunchInWithImageAsync(PunchInImageRequest req)
        {
            // Punch image APIs are based on EmployeeId (empId) directly.
            var employeeId = req.empId;
            if (employeeId <= 0)
                return "Invalid empId.";

            _logger.LogInformation(LogMessages.Attendance.ProcessingPunchIn, employeeId);

            var punchIn = PreserveReceivedDateTime(req.punchTime);
            var attendanceDate = punchIn.Date;

            var existingPunch = await _repo.GetPunchByEmployeeAndDate(employeeId, attendanceDate);
            if (existingPunch != null && existingPunch.PunchIn != null)
            {
                _logger.LogWarning(AttendanceMessages.PunchInAlreadyDone);
                return AttendanceMessages.PunchInAlreadyDone;
            }

            // Upload punch-in photo if provided.
            string? punchInImage = null;
            if (req.PunchInImage != null)
                punchInImage = await _blobService.UploadAsync(req.PunchInImage, employeeId);

            var punchId = await _repo.InsertPunchIn(
                employeeId,
                punchIn,
                attendanceDate,
                MobileSource,
                coordinateIn: null,
                linkIn: null,
                punchInImage: punchInImage
            );

            if (punchId > 0)
            {
                _logger.LogInformation(LogMessages.Attendance.PunchInSuccessful, employeeId);
                return AttendanceMessages.PunchInSuccessful;
            }

            _logger.LogWarning(AttendanceMessages.PunchInFailed);
            return AttendanceMessages.PunchInFailed;
        }

        public async Task<string> PunchOutWithImageAsync(PunchOutImageRequest req)
        {
            var employeeId = req.empId;
            if (employeeId <= 0)
                return "Invalid empId.";

            _logger.LogInformation(LogMessages.Attendance.ProcessingPunchOut, employeeId);

            var punchOut = PreserveReceivedDateTime(req.punchTime);
            var attendanceDate = punchOut.Date;

            var openPunch = await _repo.GetOpenPunchByEmployeeId(employeeId);
            if (openPunch == null || openPunch.PunchIn == null || openPunch.PunchDate.Date != attendanceDate)
            {
                _logger.LogWarning(AttendanceMessages.CannotPunchOutWithoutPunchIn);
                return AttendanceMessages.CannotPunchOutWithoutPunchIn;
            }

            if (openPunch.PunchOut != null)
            {
                _logger.LogWarning(AttendanceMessages.PunchOutAlreadyDone);
                return AttendanceMessages.PunchOutAlreadyDone;
            }

            // Upload punch-out photo if provided.
            string? punchOutImage = null;
            if (req.PunchOutImage != null)
                punchOutImage = await _blobService.UploadAsync(req.PunchOutImage, employeeId);

            var duration = CalculateDurationInMinutes(openPunch.PunchIn, punchOut);

            await _repo.UpdatePunchOut(
                openPunch.Id,
                punchOut,
                duration,
                MobileSource,
                coordinateOut: null,
                linkOut: null,
                punchOutImage: punchOutImage
            );

            _logger.LogInformation(LogMessages.Attendance.PunchOutSuccessful, employeeId);
            return AttendanceMessages.PunchOutSuccessful;
        }

        /// <summary>
        /// Mobile sends wall-clock datetimes (typically Kind=Unspecified).
        /// Store and use the exact value without server timezone conversion.
        /// </summary>
        private static DateTime PreserveReceivedDateTime(DateTime dateTime) => dateTime;

        private async Task<int> GetEmployeeTenantIdAsync(int employeeId)
        {
            var employee = await _repo.GetEmployeeByIdAsync(employeeId);
            return employee?.OrganisationId ?? 0;
        }

        private async Task<List<DayOfWeek>> GetTenantWeeklyOffDaysAsync(int tenantId)
        {
            return await _tenantWeekOffRepository.GetTenantWeeklyOffDaysAsync(tenantId);
        }

        private async Task<List<PartialWeekOffDayItem>> GetEmployeePartialWeekOffDaysAsync(int employeeId, int tenantId)
        {
            try
            {
                var employeeConfig = await _repo.GetEmployeeLevelAttendanceWeekOffAsync(employeeId, tenantId);
                if (employeeConfig == null)
                    return new List<PartialWeekOffDayItem>();

                var partialWeekOffDays = WeekOffHelper.ParsePartialWeekOffs(employeeConfig.PartialWeekOffJson);
                if (partialWeekOffDays.Count == 0
                    && !string.IsNullOrWhiteSpace(employeeConfig.PartialWeekOffJson)
                    && !string.Equals(employeeConfig.PartialWeekOffJson.Trim(), "[]", StringComparison.Ordinal))
                {
                    _logger.LogWarning(LogMessages.Attendance.InvalidEmployeePartialWeekOffJson, employeeId);
                }

                return partialWeekOffDays;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, LogMessages.Attendance.ErrorFetchingEmployeePartialWeekOff, employeeId);
                return new List<PartialWeekOffDayItem>();
            }
        }

        private void ApplyPunchFields(CalendarDayAttendance dayAttendance, AttendanceReport attendance)
        {
            dayAttendance.PunchId = attendance.PunchId ?? (attendance.Id > 0 ? attendance.Id : null);
            dayAttendance.PunchIn = attendance.PunchIn;
            dayAttendance.PunchOut = attendance.PunchOut;
            dayAttendance.WorkingHours = attendance.WorkingDuration;
            dayAttendance.InSource = attendance.InSource;
            dayAttendance.OutSource = attendance.OutSource;
            dayAttendance.CoordinateIn = attendance.CoordinateIn;
            dayAttendance.CoordinateOut = attendance.CoordinateOut;
            dayAttendance.LinkIn = attendance.LinkIn;
            dayAttendance.LinkOut = attendance.LinkOut;
            dayAttendance.PunchInImage = ToClientPunchImageUrl(attendance.PunchInImage);
            dayAttendance.PunchOutImage = ToClientPunchImageUrl(attendance.PunchOutImage);
        }

        private void ApplyPunchFields(AttendanceSummaryDetail detail, AttendanceReport attendance)
        {
            detail.PunchId = attendance.PunchId ?? (attendance.Id > 0 ? attendance.Id : null);
            detail.PunchIn = attendance.PunchIn;
            detail.PunchOut = attendance.PunchOut;
            detail.WorkingHours = attendance.WorkingDuration;
            detail.InSource = attendance.InSource;
            detail.OutSource = attendance.OutSource;
            detail.CoordinateIn = attendance.CoordinateIn;
            detail.CoordinateOut = attendance.CoordinateOut;
            detail.LinkIn = attendance.LinkIn;
            detail.LinkOut = attendance.LinkOut;
            detail.PunchInImage = ToClientPunchImageUrl(attendance.PunchInImage);
            detail.PunchOutImage = ToClientPunchImageUrl(attendance.PunchOutImage);
        }

        private double? CalculateDurationInMinutes(DateTime? punchIn, DateTime punchOut)
        {
            if (punchIn == null) return null;

            var diff = punchOut - punchIn.Value;
            return Math.Round(diff.TotalMinutes, 2);
        }

        public string GenerateGoogleMapLink(double? lat, double? lng)
        {
            if (!lat.HasValue || !lng.HasValue)
                return string.Empty;

            var latText = lat.Value.ToString(CultureInfo.InvariantCulture);
            var lngText = lng.Value.ToString(CultureInfo.InvariantCulture);
            return $"https://www.google.com/maps/search/?api=1&query={latText},{lngText}";
        }

        private static string? BuildCoordinate(double? lat, double? lng)
        {
            if (!lat.HasValue || !lng.HasValue)
                return null;

            var latText = lat.Value.ToString(CultureInfo.InvariantCulture);
            var lngText = lng.Value.ToString(CultureInfo.InvariantCulture);
            return $"{latText},{lngText}";
        }

        private async Task<string?> TryGetReverseGeocodedAddressAsync(double? lat, double? lng)
        {
            if (!lat.HasValue || !lng.HasValue)
                return null;

            var apiKey = _configuration["GoogleMaps:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            try
            {
                var client = _httpClientFactory.CreateClient();
                var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat.Value},{lng.Value}&key={apiKey}";
                using var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return null;

                using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);
                if (!document.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                    return null;

                if (results[0].TryGetProperty("formatted_address", out var address))
                    return address.GetString();

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Reverse geocoding failed for coordinates {Latitude},{Longitude}", lat, lng);
                return null;
            }
        }

        /// <summary>
        /// Get attendance report based on request parameters (daily or monthly)
        /// </summary>
        public async Task<AttendanceReportResponse> GetAttendanceReportAsync(AttendanceReportRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.Attendance.FetchingAttendanceReport);
                
                // Organization ID is now passed directly as int
                
                // Validate request
                if (request.Daily && !request.CalendarDate.HasValue)
                {
                    return new AttendanceReportResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.CalendarDateRequiredForDaily,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                if (request.Monthly && (!request.DateFrom.HasValue || !request.DateTo.HasValue))
                {
                    return new AttendanceReportResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.DateRangeRequiredForMonthly,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Resolve EmployeeId from UserId if UserId is provided
                int? employeeId = null;
                if (request.UserId.HasValue)
                {
                    employeeId = await ResolveEmployeeIdFromUserIdAsync(request.UserId.Value);
                    if (!employeeId.HasValue)
                    {
                        return new AttendanceReportResponse
                        {
                            Success = false,
                            Message = "No employee found for the specified UserId.",
                            Data = null,
                            TotalRecords = 0
                        };
                    }
                }

                // Create a modified request with EmployeeId for repository (which still uses EmployeeId internally)
                var repoRequest = new AttendanceReportRequest
                {
                    Id = request.Id,
                    BranchId = request.BranchId,
                    Daily = request.Daily,
                    Monthly = request.Monthly,
                    DateFrom = request.DateFrom,
                    DateTo = request.DateTo,
                    CalendarDate = request.CalendarDate,
                    EmployeeId = employeeId, // Use resolved EmployeeId
                    DepartmentId = request.DepartmentId,
                    organization = request.organization
                };

                // Fetch attendance data from repository
                var attendanceData = await _repo.GetAttendanceReportAsync(repoRequest);
                var attendanceList = attendanceData.ToList();
                EnsurePunchIds(attendanceList);
                ApplyImageSasUrls(attendanceList);

                // Calculate totals
                var totalWorkingHours = attendanceList
                    .Where(a => a.WorkingDuration.HasValue)
                    .Sum(a => a.WorkingDuration.Value);

                var totalWorkingDays = attendanceList
                    .Select(a => a.CalendarDate.Date)
                    .Distinct()
                    .Count();

                return new AttendanceReportResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceReportFetchedSuccessfully,
                    Data = attendanceList,
                    TotalRecords = attendanceList.Count,
                    TotalWorkingDays = totalWorkingDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingAttendanceReport);
                return new AttendanceReportResponse
                {
                    Success = false,
                    Message = string.Format(AttendanceMessages.ErrorFetchingAttendanceReport, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get attendance report for a specific employee
        /// </summary>
        public async Task<AttendanceReportResponse> GetEmployeeAttendanceAsync(int userId, DateTime dateFrom, DateTime dateTo)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new AttendanceReportResponse
                    {
                        Success = false,
                        Message = "No employee found for the specified UserId.",
                        Data = null,
                        TotalRecords = 0
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingEmployeeAttendance, employeeId.Value, dateFrom, dateTo);
                
                var attendanceData = await _repo.GetEmployeeAttendanceReportAsync(employeeId.Value, dateFrom, dateTo);
                var attendanceList = attendanceData.ToList();
                EnsurePunchIds(attendanceList);
                ApplyImageSasUrls(attendanceList);

                var totalWorkingHours = attendanceList
                    .Where(a => a.WorkingDuration.HasValue)
                    .Sum(a => a.WorkingDuration.Value);

                var totalWorkingDays = attendanceList
                    .Select(a => a.CalendarDate.Date)
                    .Distinct()
                    .Count();

                return new AttendanceReportResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceReportFetchedSuccessfully,
                    Data = attendanceList,
                    TotalRecords = attendanceList.Count,
                    TotalWorkingDays = totalWorkingDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingEmployeeAttendance, userId);
                return new AttendanceReportResponse
                {
                    Success = false,
                    Message = string.Format(AttendanceMessages.ErrorFetchingAttendanceReport, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Get real-time attendance status for today (or specified date)
        /// Shows all employees who have punched in
        /// </summary>
        public async Task<RealTimeAttendanceResponse> GetRealTimeAttendanceStatusAsync(DateTime? punchDate = null, int? organisationId = null, int? branchId = null, int? departmentId = null)
        {
            try
            {
                var targetDate = punchDate ?? DateTime.Today;
                _logger.LogInformation(LogMessages.Attendance.FetchingRealTimeStatus, targetDate);
                
                var attendanceData = await _repo.GetRealTimeAttendanceStatusAsync(targetDate, organisationId, branchId, departmentId);
                var attendanceList = attendanceData.ToList();
                ApplyImageSasUrls(attendanceList);

                var punchedIn = attendanceList.Count(a => a.IsPunchedIn);
                var punchedOut = attendanceList.Count(a => a.PunchOut.HasValue);

                return new RealTimeAttendanceResponse
                {
                    Success = true,
                    Message = AttendanceMessages.RealTimeAttendanceFetchedSuccessfully,
                    Data = attendanceList,
                    TotalPunchedIn = punchedIn,
                    TotalPunchedOut = punchedOut,
                    TotalNotPunched = 0 // This would require counting all employees - can be enhanced later
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingRealTimeStatus);
                return new RealTimeAttendanceResponse
                {
                    Success = false,
                    Message = string.Format(AttendanceMessages.ErrorFetchingRealTimeAttendance, ex.Message),
                    Data = null,
                    TotalPunchedIn = 0,
                    TotalPunchedOut = 0,
                    TotalNotPunched = 0
                };
            }
        }

        /// <summary>
        /// Get employees who are currently punched in (no punch out yet)
        /// </summary>
        public async Task<RealTimeAttendanceResponse> GetCurrentlyPunchedInAsync(DateTime? punchDate = null, int? organisationId = null, int? branchId = null, int? departmentId = null)
        {
            try
            {
                var targetDate = punchDate ?? DateTime.Today;
                _logger.LogInformation(LogMessages.Attendance.FetchingCurrentlyPunchedIn, targetDate);
                
                var attendanceData = await _repo.GetCurrentlyPunchedInAsync(targetDate, organisationId, branchId, departmentId);
                var attendanceList = attendanceData.ToList();
                ApplyImageSasUrls(attendanceList);

                return new RealTimeAttendanceResponse
                {
                    Success = true,
                    Message = AttendanceMessages.CurrentlyPunchedInFetchedSuccessfully,
                    Data = attendanceList,
                    TotalPunchedIn = attendanceList.Count,
                    TotalPunchedOut = 0,
                    TotalNotPunched = 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingCurrentlyPunchedIn);
                return new RealTimeAttendanceResponse
                {
                    Success = false,
                    Message = string.Format(AttendanceMessages.ErrorFetchingRealTimeAttendance, ex.Message),
                    Data = null,
                    TotalPunchedIn = 0,
                    TotalPunchedOut = 0,
                    TotalNotPunched = 0
                };
            }
        }

        /// <summary>
        /// Get attendance by calendar for a specific employee, month, and year
        /// Returns calendar-style data with attendance status for each day
        /// </summary>
        public async Task<CalendarAttendanceResponse> GetAttendanceByCalendarAsync(int userId, int month, int year)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new CalendarAttendanceResponse
                    {
                        Success = false,
                        Message = "No employee found for the specified UserId."
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingCalendarAttendance, employeeId.Value, month, year);
                
                // Validate parameters
                if (month < 1 || month > 12)
                {
                    return new CalendarAttendanceResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.InvalidMonth
                    };
                }

                if (year < 2000 || year > 2100)
                {
                    return new CalendarAttendanceResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.InvalidYear
                    };
                }

                // Get employee details
                var employee = await _repo.GetEmployeeByIdAsync(employeeId.Value);
                if (employee == null)
                {
                    return new CalendarAttendanceResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.EmployeeNotFound
                    };
                }

                // Get attendance data for the month
                var attendanceData = await _repo.GetAttendanceByCalendarAsync(employeeId.Value, month, year);
                var attendanceList = attendanceData.ToList();
                EnsurePunchIds(attendanceList);
                var attendanceDict = attendanceList.ToDictionary(a => a.CalendarDate.Date, a => a);

                var weeklyOffDays = await GetTenantWeeklyOffDaysAsync(employee.OrganisationId);
                var partialWeekOffDays = await GetEmployeePartialWeekOffDaysAsync(employeeId.Value, employee.OrganisationId);

                // Build calendar data
                var dateFrom = new DateTime(year, month, 1);
                var dateTo = dateFrom.AddMonths(1).AddDays(-1);
                var totalDays = dateTo.Day;
                var today = DateTime.Today;

                // Preload holiday dates and approved leave dates for the month
                var holidayDates = await _repo.GetHolidayDatesAsync(employee.OrganisationId, dateFrom, dateTo);
                var holidaySet = new HashSet<DateTime>(holidayDates.Select(d => d.Date));

                var leaveRanges = await _repo.GetApprovedLeaveDateRangesAsync(employeeId.Value, dateFrom, dateTo);
                var leaveSet = new HashSet<DateTime>();
				foreach (var (from, to) in leaveRanges)
				{
					var start = from.Date < dateFrom ? dateFrom : from.Date;
					var end = to.Date > dateTo ? dateTo : to.Date;

					for (var d = start; d <= end; d = d.AddDays(1))
					{
						leaveSet.Add(d);
					}
				}

				var calendarData = new List<CalendarDayAttendance>();
                int presentDays = 0;
                int absentDays = 0;
                int weekendDays = 0;
                int leaveDays = 0;
                int holidayDays = 0;
                double totalWorkingHours = 0;

                for (int day = 1; day <= totalDays; day++)
                {
                    var currentDate = new DateTime(year, month, day);
                    var isCompleteWeekOff = weeklyOffDays.Contains(currentDate.DayOfWeek);
                    var isPartialWeekOff = !isCompleteWeekOff && WeekOffHelper.IsPartialWeekOff(currentDate, partialWeekOffDays);
                    var dayAttendance = new CalendarDayAttendance
                    {
                        Date = currentDate,
                        Day = day,
                        DayName = currentDate.DayOfWeek.ToString(),
                        IsWeekend = isCompleteWeekOff || isPartialWeekOff
                    };

                    // Priority: Week Off -> Partial Week Off -> Holiday -> Leave -> Future -> Present -> Absent
                    if (isCompleteWeekOff)
                    {
                        dayAttendance.Status = "Week Off";
                        dayAttendance.IsAbsent = false;
                        weekendDays++;
                    }
                    else if (isPartialWeekOff)
                    {
                        dayAttendance.Status = "Partial Week Off";
                        dayAttendance.IsAbsent = false;
                        weekendDays++;

                        if (attendanceDict.TryGetValue(currentDate, out var partialWeekOffAttendance))
                        {
                            ApplyPunchFields(dayAttendance, partialWeekOffAttendance);
                        }
                    }
                    else if (holidaySet.Contains(currentDate.Date))
                    {
                        dayAttendance.IsHoliday = true;
                        dayAttendance.Status = "Holiday";
                        dayAttendance.IsAbsent = false;
                        holidayDays++;
                    }
                    else if (leaveSet.Contains(currentDate.Date))
                    {
                        dayAttendance.IsLeave = true;
                        dayAttendance.Status = "Leave";
                        dayAttendance.IsAbsent = false;
                        leaveDays++;
                    }
                    else if (currentDate > today)
                    {
                        dayAttendance.Status = "Future";
                        dayAttendance.IsAbsent = false;
                    }
                    else if (attendanceDict.TryGetValue(currentDate, out var attendance))
                    {
                        ApplyPunchFields(dayAttendance, attendance);

                        var hasPunchIn = attendance.PunchIn.HasValue;
                        var hasPunchOut = attendance.PunchOut.HasValue;

                        if (hasPunchIn && hasPunchOut)
                        {
                            dayAttendance.IsPresent = true;
                            dayAttendance.Status = "Present";
                            presentDays++;
                            if (attendance.WorkingDuration.HasValue)
                            {
                                totalWorkingHours += attendance.WorkingDuration.Value;
                            }
                        }
                        else if (hasPunchIn && !hasPunchOut)
                        {
                            dayAttendance.IsPresent = true;
                            dayAttendance.Status = "Present";
                            dayAttendance.Remarks = "Missing Punch Out";
                            presentDays++;
                            if (attendance.WorkingDuration.HasValue)
                            {
                                totalWorkingHours += attendance.WorkingDuration.Value;
                            }
                        }
                        else
                        {
                            dayAttendance.IsAbsent = true;
                            dayAttendance.Status = "Absent";
                            absentDays++;
                        }
                    }
                    else
                    {
                        dayAttendance.IsAbsent = true;
                        dayAttendance.Status = "Absent";
                        absentDays++;
                    }

                    calendarData.Add(dayAttendance);
                }

                var workingDays = totalDays - weekendDays;
                var monthName = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);

                return new CalendarAttendanceResponse
                {
                    Success = true,
                    Message = AttendanceMessages.CalendarAttendanceFetchedSuccessfully,
                    EmployeeName = employee.Name,
                    EmployeeNumber = employee.EmployeeNumber,
                    Month = month,
                    MonthName = monthName,
                    Year = year,
                    
                    TotalDays = totalDays,
                    WorkingDays = workingDays,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    LeaveDays = leaveDays,
                    HolidayDays = holidayDays,
                    WeekendDays = weekendDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2),
                    CalendarData = calendarData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingCalendarAttendance, userId);
                return new CalendarAttendanceResponse
                {
                    Success = false,
                    Message = string.Format(AttendanceMessages.ErrorFetchingCalendarAttendance, ex.Message)
                };
            }
        }

        /// <summary>
        /// Get attendance summary for a specific employee within a date range
        /// </summary>
        public async Task<AttendanceSummaryResponse> GetAttendanceSummaryAsync(int organizationId, int userId, DateTime fromDate, DateTime toDate)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new AttendanceSummaryResponse
                    {
                        Success = false,
                        Message = "No employee found for the specified UserId."
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingAttendanceSummary, employeeId.Value, fromDate, toDate);
                
                // Validate parameters
                if (fromDate > toDate)
                {
                    return new AttendanceSummaryResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.InvalidDateRange
                    };
                }

                // Get employee details
                var employee = await _repo.GetEmployeeByIdAsync(employeeId.Value);
                if (employee == null)
                {
                    return new AttendanceSummaryResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.EmployeeNotFound
                    };
                }

                // Get attendance data for the date range
                var attendanceData = await _repo.GetEmployeeAttendanceReportAsync(employeeId.Value, fromDate, toDate);
                var attendanceListForSummary = attendanceData.ToList();
                EnsurePunchIds(attendanceListForSummary);
                var attendanceDict = attendanceListForSummary.ToDictionary(a => a.CalendarDate.Date, a => a);

                var weeklyOffDays = await GetTenantWeeklyOffDaysAsync(employee.OrganisationId);
                var partialWeekOffDays = await GetEmployeePartialWeekOffDaysAsync(employeeId.Value, employee.OrganisationId);

                // Build summary data
                var totalDays = (toDate - fromDate).Days + 1;
                var today = DateTime.Today;

                var attendanceDetails = new List<AttendanceSummaryDetail>();
                int presentDays = 0;
                int absentDays = 0;
                int weekendDays = 0;
                double totalWorkingHours = 0;

                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    var detail = new AttendanceSummaryDetail
                    {
                        Date = date,
                        DayName = date.DayOfWeek.ToString()
                    };

                    var isCompleteWeekOff = weeklyOffDays.Contains(date.DayOfWeek);
                    var isPartialWeekOff = !isCompleteWeekOff && WeekOffHelper.IsPartialWeekOff(date, partialWeekOffDays);

                    if (isCompleteWeekOff)
                    {
                        detail.Status = "Week Off";
                        weekendDays++;
                    }
                    else if (isPartialWeekOff)
                    {
                        detail.Status = "Partial Week Off";
                        weekendDays++;

                        if (attendanceDict.TryGetValue(date, out var partialWeekOffAttendance))
                        {
                            ApplyPunchFields(detail, partialWeekOffAttendance);
                        }
                    }
                    else if (date > today)
                    {
                        detail.Status = "Future";
                    }
                    else if (attendanceDict.TryGetValue(date, out var attendance))
                    {
                        ApplyPunchFields(detail, attendance);
                        detail.Status = "Present";
                        presentDays++;
                        
                        if (attendance.WorkingDuration.HasValue)
                        {
                            totalWorkingHours += attendance.WorkingDuration.Value;
                        }
                    }
                    else
                    {
                        detail.Status = "Absent";
                        absentDays++;
                    }

                    attendanceDetails.Add(detail);
                }

                var workingDays = totalDays - weekendDays;
                var averageWorkingHours = presentDays > 0 ? Math.Round(totalWorkingHours / presentDays, 2) : 0;

                return new AttendanceSummaryResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceSummaryFetchedSuccessfully,
                    OrganizationId = organizationId,
                    EmployeeName = employee.Name,
                    EmployeeNumber = employee.EmployeeNumber,
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalDays = totalDays,
                    WorkingDays = workingDays,
                    PresentDays = presentDays,
                    AbsentDays = absentDays,
                    LeaveDays = 0,
                    HolidayDays = 0,
                    WeekendDays = weekendDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2),
                    AverageWorkingHours = averageWorkingHours,
                    AttendanceDetails = attendanceDetails
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingAttendanceSummary, userId);
                return new AttendanceSummaryResponse
                {
                    Success = false,
                    Message = string.Format(AttendanceMessages.ErrorFetchingAttendanceSummary, ex.Message)
                };
            }
        }

        /// <summary>
        /// Get attendance reports by organisation ID (current month by default)
        /// </summary>
        public async Task<AttendanceReportResponse> GetAttendanceReportsByOrganisationAsync(int organisationId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Attendance.FetchingAttendanceReportsForOrganisation, organisationId);
                
                // Validate parameters
                if (organisationId <= 0)
                {
                    return new AttendanceReportResponse
                    {
                        Success = false,
                        Message = OrganisationMessages.OrganisationIdRequired,
                        Data = null,
                        TotalRecords = 0
                    };
                }

                // Get current month's date range
                var today = DateTime.Today;
                var dateFrom = new DateTime(today.Year, today.Month, 1);
                var dateTo = dateFrom.AddMonths(1).AddDays(-1);

                // Fetch attendance data from repository
                var attendanceData = await _repo.GetAttendanceReportsByOrganisationAsync(organisationId, dateFrom, dateTo);
                var attendanceList = attendanceData.ToList();
                EnsurePunchIds(attendanceList);
                ApplyImageSasUrls(attendanceList);

                // Calculate totals
                var totalWorkingHours = attendanceList
                    .Where(a => a.WorkingDuration.HasValue)
                    .Sum(a => a.WorkingDuration.Value);

                var totalWorkingDays = attendanceList
                    .Select(a => a.CalendarDate.Date)
                    .Distinct()
                    .Count();

                return new AttendanceReportResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceReportFetchedSuccessfully,
                    Data = attendanceList,
                    TotalRecords = attendanceList.Count,
                    TotalWorkingDays = totalWorkingDays,
                    TotalWorkingHours = Math.Round(totalWorkingHours, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorFetchingAttendanceReportsForOrganisation, organisationId);
                return new AttendanceReportResponse
                {
                    Success = false,
                    Message = string.Format(AttendanceMessages.ErrorFetchingAttendanceReport, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        /// <summary>
        /// Deletes an attendance (Punch) record.
        /// </summary>
        public async Task<AttendanceDeleteResponse> DeleteAttendanceAsync(int id, int tenantId)
        {
            try
            {
                _logger.LogInformation(LogMessages.Attendance.DeletingAttendanceRecord, id);

                if (id <= 0)
                {
                    return new AttendanceDeleteResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.PunchIdRequired,
                        Data = null
                    };
                }

                var existingPunch = await _repo.GetPunchByIdAsync(id, tenantId);
                if (existingPunch == null)
                {
                    return new AttendanceDeleteResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.AttendanceNotFound,
                        Data = null
                    };
                }

                var employee = await _repo.GetEmployeeByIdAsync(existingPunch.EmployeeId);
                if (employee == null)
                {
                    return new AttendanceDeleteResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.EmployeeNotFound,
                        Data = null
                    };
                }

                var deleted = await _repo.DeletePunchAsync(id, tenantId);

                if (deleted)
                {
                    return new AttendanceDeleteResponse
                    {
                        Success = true,
                        Message = AttendanceMessages.AttendanceDeletedSuccessfully,
                        Data = new { PunchId = id }
                    };
                }

                // Race: punch was deleted between existence check and delete
                return new AttendanceDeleteResponse
                {
                    Success = false,
                    Message = AttendanceMessages.AttendanceNotFound,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorDeletingAttendanceRecord, id);
                return new AttendanceDeleteResponse
                {
                    Success = false,
                    Message = AttendanceMessages.FailedToDeleteAttendance,
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get attendance status for an employee on a specific date
        /// </summary>
        public async Task<AttendanceStatusResponse> GetAttendanceStatusAsync(int userId, DateTime date, int tenantId)
        {
            try
            {
                // Resolve EmployeeId from UserId
                var employeeId = await ResolveEmployeeIdFromUserIdAsync(userId);
                if (!employeeId.HasValue)
                {
                    return new AttendanceStatusResponse
                    {
                        Success = false,
                        Message = "No employee found for the specified UserId.",
                        Data = null
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.GettingAttendanceStatus, employeeId.Value, date);

                // Validate parameters
                if (date == default(DateTime))
                {
                    return new AttendanceStatusResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.DateRequired,
                        Data = null
                    };
                }

                // Get punch record with tenant filter
                var punch = await _repo.GetPunchByEmployeeAndDateWithTenant(employeeId.Value, date, tenantId);

                var statusData = new AttendanceStatusData
                {
                    date = date.Date
                };

                if (punch != null)
                {
                    statusData.PunchId = punch.Id > 0 ? punch.Id : null;
                    statusData.isMarked = punch.PunchIn.HasValue;
                    statusData.inSource = punch.InSource;
                    statusData.outSource = punch.OutSource;
                    statusData.coordinateIn = punch.CoordinateIn;
                    statusData.coordinateOut = punch.CoordinateOut;
                    statusData.linkIn = punch.LinkIn;
                    statusData.linkOut = punch.LinkOut;
                    statusData.punchInImage = ToClientPunchImageUrl(punch.PunchInImage);
                    statusData.punchOutImage = ToClientPunchImageUrl(punch.PunchOutImage);
                    statusData.punchIn = punch.PunchIn;
                    statusData.punchOut = punch.PunchOut;
                    statusData.duration = punch.Duration;
                    statusData.isAlreadyMarked = punch.PunchIn.HasValue;
                    statusData.status = punch.PunchIn.HasValue
                        ? (punch.PunchOut.HasValue ? "Present" : "Present")
                        : "Absent";
                }
                else
                {
                    // Attendance not marked
                    statusData.isMarked = false;
                    statusData.isAlreadyMarked = false;
                    statusData.status = "Not Marked";
                    statusData.punchIn = null;
                    statusData.punchOut = null;
                }

                return new AttendanceStatusResponse
                {
                    Success = true,
                    Message = AttendanceMessages.AttendanceStatusRetrievedSuccessfully,
                    Data = statusData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.Attendance.ErrorGettingAttendanceStatus, userId, date);
                return new AttendanceStatusResponse
                {
                    Success = false,
                    Message = $"Error retrieving attendance status: {ex.Message}",
                    Data = null
                };
            }
        }

        /// <summary>
        /// Get today's punch in / punch out logs for the current user (merges DeviceLog + Punch table).
        /// </summary>
        public async Task<TodayPunchLogsResponse> GetTodayPunchLogsAsync(int userId, int tenantId)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeebyUserIdAsync(userId);
                if (employee == null)
                {
                    return new TodayPunchLogsResponse
                    {
                        Success = false,
                        Message = EmployeeMessages.EmployeeNotFoundForUserId,
                        Data = null
                    };
                }

                // Ensure tenant isolation (authenticated user must belong to the requested tenant)
                if (employee.OrganisationId != tenantId)
                {
                    return new TodayPunchLogsResponse
                    {
                        Success = false,
                        Message = TenantAccessMessages.TenantAccessDenied,
                        Data = null
                    };
                }

                _logger.LogInformation(LogMessages.Attendance.FetchingTodayPunchLogs, userId);

                var today = DateTime.Today;

                var deviceLogs = await _repo.GetTodayPunchLogsAsync(employee.BiometricNumber, today);
                var punchLogs = await _repo.GetTodayPunchLogsFromPunchAsync(employee.Id, tenantId, today);

                var logList = deviceLogs
                    .Concat(punchLogs)
                    .OrderBy(l => l.LogDateTime)
                    .ToList();
                ApplyImageSasUrls(logList);

                return new TodayPunchLogsResponse
                {
                    Success = true,
                    Message = AttendanceMessages.TodayPunchLogsFetchedSuccessfully,
                    Data = logList
                };
            }
            catch (Exception ex)
            {
                _logger.LogException(ExceptionCodes.Attendance.GetTodayPunchLogs, nameof(GetTodayPunchLogsAsync), ex, userId);
                return new TodayPunchLogsResponse
                {
                    Success = false,
                    Message = GeneralMessages.SomethingWentWrongContactAdmin,
                    Data = null
                };
            }
        }
    }
}
