using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;
using MobileWebApi.Helper;

namespace MobileWebApi.Controllers
{
    [Route("attendance")]
    [ApiController]
    [Authorize]
    public class AttendanceController : TenantBaseController
    {
        private readonly IAttendanceService _service;
        private readonly IEmployeeService _employeeService;
        private readonly IAttendanceOverviewService _attendanceOverviewService;

        public AttendanceController(
            IAttendanceService service,
            IEmployeeService employeeService,
            IAttendanceOverviewService attendanceOverviewService,
            ITenantContext tenantContext,
            ILogger<AttendanceController> logger) 
            : base(tenantContext, logger)
        {
            _service = service;
            _employeeService = employeeService;
            _attendanceOverviewService = attendanceOverviewService;
        }

		/// <summary>
		/// Validates that the provided date matches today's date (server local time).
		/// Returns BadRequest if the date is not today, otherwise returns null.
		/// </summary>
		private IActionResult? ValidateDateIsToday(DateTime dateToValidate, string operationType)
		{
			// Log received value for debugging
			Logger.LogInformation(
				"Validating {OperationType}. Received Date: {Date}, Kind: {Kind}",
				operationType,
				dateToValidate,
				dateToValidate.Kind);

			DateTime requestDate;

			switch (dateToValidate.Kind)
			{
				case DateTimeKind.Utc:
					requestDate = dateToValidate.ToLocalTime().Date;
					break;

				case DateTimeKind.Local:
					requestDate = dateToValidate.Date;
					break;

				case DateTimeKind.Unspecified:
					// Treat as local date to avoid incorrect timezone conversion
					requestDate = dateToValidate.Date;
					break;

				default:
					requestDate = dateToValidate.Date;
					break;
			}

			DateTime todayDate = DateTime.Now.Date;

			Logger.LogInformation(
				"Operation: {OperationType}, Request Date: {RequestDate}, Today: {TodayDate}",
				operationType,
				requestDate,
				todayDate);

			if (requestDate != todayDate)
			{
				Logger.LogWarning(
					"Invalid date for {OperationType}: Requested date {RequestDate} does not match today's date {TodayDate}",
					operationType,
					requestDate,
					todayDate);

				return BadRequest(new
				{
					Success = false,
					Message = "Punch in/out is allowed only for today's date."
				});
			}

			return null;
		}
		/// <summary>
		/// Validates that the current user can access the specified employee's data.
		/// HR/TenantAdmin can access all employees. Regular users can only access their own data.
		/// </summary>
		private async Task<IActionResult?> ValidateEmployeeAccessAsync(int employeeId)
        {
            // HR or TenantAdmin can access all employees
            if (HasElevatedAccess)
            {
                return null; // Access granted
            }

            // For regular users, lookup employee to get their SystemUserId
            var employeeResult = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (!employeeResult.Success || employeeResult.Data == null)
            {
                return NotFound(new { Success = false, Message = AttendanceMessages.EmployeeNotFound });
            }

            // Check if the employee belongs to the current user
            if (employeeResult.SystemUserId != CurrentUserId)
            {
                Logger.LogWarning(LogMessages.TenantAccess.UserAttemptedAccessAttendance,
                    CurrentUserId, employeeId, employeeResult.SystemUserId);
                return UserAccessDenied();
            }

            return null; // Access granted
        }

		/// <summary>
		/// Punch In
		/// POST: attendance/punch-in
		/// </summary>
		[HttpPost("punch-in")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> PunchIn([FromForm] PunchInRequest request)
		{
            try
            {
                if (request == null)
                {
                    return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
                }

                // Validate UserId
                if (request.userId <= 0)
                {
                    return BadRequest(new { Success = false, Message = "UserId is required." });
                }

                // Enforce: users can punch in only for themselves (no punching for other employees)
                if (request.userId != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UserAttemptedPunchIn,
                        CurrentUserId, request.userId, request.userId);
                    return UserAccessDenied();
                }

                // Validate that attendance_date is today (server local date)
                var dateValidation = ValidateDateIsToday(request.attendance_date, "Punch In");
                if (dateValidation != null)
                {
                    return dateValidation;
                }

                // Also validate punch_in_time date part matches today
                var timeValidation = ValidateDateIsToday(request.punch_in_time, "Punch In");
                if (timeValidation != null)
                {
                    return timeValidation;
                }

                Logger.LogInformation(LogMessages.Attendance.ProcessingPunchIn, request.userId);
                var result = await _service.PunchInAsync(request);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.PunchIn, nameof(PunchIn), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        ///// <summary>
        ///// Punch In with optional image upload.
        ///// Endpoint consumes multipart/form-data to support IFormFile.
        ///// POST: attendance/punch-in (multipart/form-data)
        ///// </summary>
        //[HttpPost("punch-in-with-image")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> PunchInWithImage([FromForm] PunchInImageRequest request)
        //{
        //    try
        //    {
        //        if (request == null)
        //            return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });

        //        if (request.empId <= 0)
        //            return BadRequest(new { Success = false, Message = "empId is required." });

        //        // Enforce: regular users can punch only for their own employees.
        //        var accessValidation = await ValidateEmployeeAccessAsync(request.empId);
        //        if (accessValidation != null)
        //            return accessValidation;

        //        // Validate that punchTime is today's date (server local date).
        //        var dateValidation = ValidateDateIsToday(request.punchTime, "Punch In");
        //        if (dateValidation != null)
        //            return dateValidation;

        //        Logger.LogInformation(LogMessages.Attendance.ProcessingPunchIn, request.empId);
        //        var result = await _service.PunchInWithImageAsync(request);
        //        return Ok(new { message = result });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { Success = false, Message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException(ExceptionCodes.Attendance.PunchIn, nameof(PunchInWithImage), ex, request.empId);
        //        return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
        //    }
        //}

		/// <summary>
		/// Punch Out
		/// POST: attendance/punch-out
		/// </summary>
		[HttpPost("punch-out")]
		[Consumes("multipart/form-data")]
		public async Task<IActionResult> PunchOut([FromForm] PunchOutRequest request)
		{
            try
            {
                if (request == null)
                {
                    return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
                }

                // Validate UserId
                if (request.userId <= 0)
                {
                    return BadRequest(new { Success = false, Message = "UserId is required." });
                }

                // Enforce: users can punch out only for themselves (no punching for other employees)
                if (request.userId != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UserAttemptedPunchOut,
                        CurrentUserId, request.userId, request.userId);
                    return UserAccessDenied();
                }

                // Validate that attendance_date is today (server local date)
                var dateValidation = ValidateDateIsToday(request.attendance_date, "Punch Out");
                if (dateValidation != null)
                {
                    return dateValidation;
                }

                // Also validate punch_out_time date part matches today
                var timeValidation = ValidateDateIsToday(request.punch_out_time, "Punch Out");
                if (timeValidation != null)
                {
                    return timeValidation;
                }

                Logger.LogInformation(LogMessages.Attendance.ProcessingPunchOut, request.userId);
                var result = await _service.PunchOutAsync(request);
                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.PunchOut, nameof(PunchOut), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        ///// <summary>
        ///// Punch Out with optional image upload.
        ///// Endpoint consumes multipart/form-data to support IFormFile.
        ///// POST: attendance/punch-out (multipart/form-data)
        ///// </summary>
        //[HttpPost("punch-out-with-image")]
        //[Consumes("multipart/form-data")]
        //public async Task<IActionResult> PunchOutWithImage([FromForm] PunchOutImageRequest request)
        //{
        //    try
        //    {
        //        if (request == null)
        //            return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });

        //        if (request.empId <= 0)
        //            return BadRequest(new { Success = false, Message = "empId is required." });

        //        // Enforce: regular users can punch only for their own employees.
        //        var accessValidation = await ValidateEmployeeAccessAsync(request.empId);
        //        if (accessValidation != null)
        //            return accessValidation;

        //        // Validate that punchTime is today's date (server local date).
        //        var dateValidation = ValidateDateIsToday(request.punchTime, "Punch Out");
        //        if (dateValidation != null)
        //            return dateValidation;

        //        Logger.LogInformation(LogMessages.Attendance.ProcessingPunchOut, request.empId);
        //        var result = await _service.PunchOutWithImageAsync(request);
        //        return Ok(new { message = result });
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(new { Success = false, Message = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogException(ExceptionCodes.Attendance.PunchOut, nameof(PunchOutWithImage), ex, request.empId);
        //        return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
        //    }
        //}

        /// <summary>
        /// Get today's attendance for all employees in the organization
        /// GET: attendance/get-all-attendance
        /// Note: Returns attendance for all employees in the user's organization for today.
        /// </summary>
        [HttpGet("get-all-attendance")]
        public async Task<IActionResult> GetAttendance()
        {
            try
            {
                var today = DateTime.Today;
                var organizationId = CurrentOrganisationId;

                Logger.LogInformation(LogMessages.Attendance.FetchingAttendance, today);

                var request = new AttendanceReportRequest
                {
                    Daily = true,
                    Monthly = false,
                    CalendarDate = today,
                    organization = organizationId
                    // EmployeeId is null to get all employees in the organization
                };

                var result = await _service.GetAttendanceReportAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.GetAttendanceReport, nameof(GetAttendance), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        /// <summary>
        /// Get attendance by personal details for employee (current month)
        /// GET: attendance/get-attendance-by-personal-details/?user_id=7
        /// Note: Regular users can only see their own attendance. HR/TenantAdmin can see all.
        /// Workdays: both punches → Present; punch in only → Present with Remarks "Missing Punch Out" (PunchIn still returned); neither punch → Absent.
        /// </summary>
        [HttpGet("get-attendance-by-personal-details")]
        public async Task<IActionResult> GetAttendanceByPersonalDetails([FromQuery] int user_id)
        {
            try
            {
                // Validate UserId
                if (user_id <= 0)
                {
                    return BadRequest(new { Success = false, Message = "UserId is required." });
                }

                // Enforce: users can only see their own attendance (unless HR/TenantAdmin)
                if (!HasElevatedAccess && user_id != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UserAttemptedAccessAttendance,
                        CurrentUserId, user_id, user_id);
                    return UserAccessDenied();
                }

                var currentDate = DateTime.Now;
                Logger.LogInformation(LogMessages.Attendance.FetchingAttendanceByPersonalDetails, user_id);

                var result = await _service.GetAttendanceByCalendarAsync(user_id, currentDate.Month, currentDate.Year);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.GetCalendarAttendance, nameof(GetAttendanceByPersonalDetails), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        //[HttpPost("Attendance-Report")]
        //public async Task<IActionResult> GetAttendanceReport([FromBody] AttendanceReportRequest request)
        //{
        //    Logger.LogInformation(LogMessages.Attendance.FetchingAttendanceReport);

        //    if (request == null)
        //    {
        //        return BadRequest(new AttendanceReportResponse
        //        {
        //            Success = false,
        //            Message = GeneralMessages.RequestBodyCannotBeNull,
        //            Data = null,
        //            TotalRecords = 0
        //        });
        //    }

        //    var result = await _service.GetAttendanceReportAsync(request);

        //    if (result.Success)
        //    {
        //        return Ok(result);
        //    }

        //    return BadRequest(result);
        //}

        /// <summary>
        /// Get employee attendance report within date range
        /// Note: Regular users can only see their own attendance. HR/TenantAdmin can see all.
        /// </summary>
        //[HttpGet("employee-attendance-report")]
        //public async Task<IActionResult> GetEmployeeAttendanceReport(
        //    [FromQuery] int employee_id,
        //    [FromQuery] DateTime from_date,
        //    [FromQuery] DateTime to_date)
        //{
        //    if (employee_id <= 0)
        //    {
        //        return BadRequest(new AttendanceReportResponse
        //        {
        //            Success = false,
        //            Message = AttendanceMessages.EmployeeIdRequired,
        //            Data = null,
        //            TotalRecords = 0
        //        });
        //    }

        //    if (from_date > to_date)
        //    {
        //        return BadRequest(new AttendanceReportResponse
        //        {
        //            Success = false,
        //            Message = AttendanceMessages.InvalidDateRange,
        //            Data = null,
        //            TotalRecords = 0
        //        });
        //    }

        //    // Validate user access - regular users can only see their own attendance
        //    var accessDenied = await ValidateEmployeeAccessAsync(employee_id);
        //    if (accessDenied != null)
        //    {
        //        return accessDenied;
        //    }

        //    Logger.LogInformation(LogMessages.Attendance.FetchingEmployeeAttendance, employee_id, from_date, to_date);
        //    var result = await _service.GetEmployeeAttendanceAsync(employee_id, from_date, to_date);

        //    if (result.Success)
        //    {
        //        return Ok(result);
        //    }

        //    return BadRequest(result);
        //}
        /// <summary>
        /// Get attendance summary for employee
        /// Note: Regular users can only see their own attendance. HR/TenantAdmin can see all.
        /// </summary>
        [HttpGet("/apipunch/attendance/get-attendance-summery")]
        public async Task<IActionResult> GetAttendanceSummary(
            [FromQuery] int organization_id,
            [FromQuery] int user_id,
            [FromQuery] DateTime from_date,
            [FromQuery] DateTime to_date)
        {
            try
            {
                var validatedOrgIdForAccess = GetValidatedOrganisationId(organization_id);

                // Validate UserId
                if (user_id <= 0)
                {
                    return BadRequest(new { Success = false, Message = "UserId is required." });
                }

                // Validate tenant access - user can only access their own organisation's data
                var validatedOrgId = validatedOrgIdForAccess;

                // Enforce: users can only see their own attendance (unless HR/TenantAdmin)
                if (!HasElevatedAccess && user_id != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UserAttemptedAccessAttendance,
                        CurrentUserId, user_id, user_id);
                    return UserAccessDenied();
                }

                Logger.LogInformation(LogMessages.Attendance.FetchingAttendanceSummary, user_id, from_date, to_date);

                var result = await _service.GetAttendanceSummaryAsync(validatedOrgId, user_id, from_date, to_date);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.GetAttendanceSummary, nameof(GetAttendanceSummary), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        /// <summary>
        /// Get attendance by calendar for employee
        /// GET: apipunch/attendance/get-attendance-by-calendar/?user_id=10&month=01&year=2025
        /// Note: Regular users can only see their own attendance. HR/TenantAdmin can see all.
        /// </summary>
        [HttpGet("/apipunch/attendance/get-attendance-by-calendar")]
        public async Task<IActionResult> GetAttendanceByCalendar(
            [FromQuery] int user_id,
            [FromQuery] int month,
            [FromQuery] int year)
        {
            try
            {
                // Validate UserId
                if (user_id <= 0)
                {
                    return BadRequest(new { Success = false, Message = "UserId is required." });
                }

                // Enforce: users can only see their own attendance (unless HR/TenantAdmin)
                if (!HasElevatedAccess && user_id != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UserAttemptedAccessAttendance,
                        CurrentUserId, user_id, user_id);
                    return UserAccessDenied();
                }

                Logger.LogInformation(LogMessages.Attendance.FetchingCalendarAttendance, user_id, month, year);

                var result = await _service.GetAttendanceByCalendarAsync(user_id, month, year);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.GetCalendarAttendance, nameof(GetAttendanceByCalendar), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        /// <summary>
        /// Get attendance reports by organization
        /// GET: apipunch/report/attendance/get_reports/?org_id=1
        /// Note: org_id parameter is validated against user's tenant. Users can only access their own organisation's data.
        /// </summary>
        //[HttpGet("/apipunch/report/attendance/get_reports")]
        //public async Task<IActionResult> GetReportsByOrganisation([FromQuery] int org_id)
        //{
        //    // Validate tenant access - user can only access their own organisation's data
        //    var validatedOrgId = GetValidatedOrganisationId(org_id);
            
        //    Logger.LogInformation("Fetching attendance reports for organisation {OrganisationId}", validatedOrgId);
            
        //    var result = await _service.GetAttendanceReportsByOrganisationAsync(validatedOrgId);
            
        //    if (result.Success)
        //    {
        //        return Ok(result);
        //    }
            
        //    return BadRequest(result);
        //}

        /// <summary>
        /// Get attendance report (daily or monthly) by query parameters
        /// GET: /apipunch/report/attendance/?branch_id=1&monthly=true&date_from=2024-01-01&date_to=2024-01-31
        /// For daily report: /apipunch/report/attendance/?branch_id=1&monthly=false&calendar_date=2024-01-15
        /// </summary>
     
        //[HttpGet("/apipunch/report/attendance/")]
        //public async Task<IActionResult> GetAttendanceReportByQuery(
        //    [FromQuery] int? branch_id,
        //    [FromQuery] bool monthly = false,
        //    [FromQuery] DateTime? date_from = null,
        //    [FromQuery] DateTime? date_to = null,
        //    [FromQuery] DateTime? calendar_date = null,
        //    [FromQuery] int? employee_id = null,
        //    [FromQuery] int? department_id = null)
        //{
        //    Logger.LogInformation(LogMessages.Attendance.FetchingAttendanceReport);

        //    // Build request object
        //    var request = new AttendanceReportRequest
        //    {
        //        BranchId = branch_id,
        //        Monthly = monthly,
        //        Daily = !monthly,
        //        DateFrom = date_from,
        //        DateTo = date_to,
        //        CalendarDate = calendar_date,
        //        EmployeeId = employee_id,
        //        DepartmentId = department_id,
        //        organization = CurrentOrganisationId
        //    };

        //    // Validate request based on report type
        //    if (monthly)
        //    {
        //        if (!date_from.HasValue || !date_to.HasValue)
        //        {
        //            return BadRequest(new AttendanceReportResponse
        //            {
        //                Success = false,
        //                Message = AttendanceMessages.DateRangeRequiredForMonthly,
        //                Data = null,
        //                TotalRecords = 0
        //            });
        //        }

        //        if (date_from.Value > date_to.Value)
        //        {
        //            return BadRequest(new AttendanceReportResponse
        //            {
        //                Success = false,
        //                Message = AttendanceMessages.InvalidDateRange,
        //                Data = null,
        //                TotalRecords = 0
        //            });
        //        }
        //    }
        //    else // Daily report
        //    {
        //        if (!calendar_date.HasValue)
        //        {
        //            // Default to today if not provided
        //            request.CalendarDate = DateTime.Today;
        //        }
        //    }

        //    var result = await _service.GetAttendanceReportAsync(request);

        //    if (result.Success)
        //    {
        //        return Ok(result);
        //    }

        //    return BadRequest(result);
        //}

        /// <summary>
        /// Delete attendance record
        /// DELETE: attendance/delete-attendance/?id=4
        /// </summary>
        [HttpDelete("delete-attendance")]
        public async Task<IActionResult> DeleteAttendance([FromQuery] int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new AttendanceDeleteResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.EmployeeIdRequired,
                        Data = null
                    });
                }

                var tenantId = CurrentOrganisationId;

                Logger.LogInformation(LogMessages.Attendance.DeletingAttendanceRecord, id);
                var result = await _service.DeleteAttendanceAsync(id, tenantId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.DeleteAttendance, nameof(DeleteAttendance), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        /// <summary>
        /// Get attendance status for an employee on a specific date
        /// GET: api/attendance/status?userId=10&date=2025-01-15
        /// Note: Regular users can only check their own attendance. HR/TenantAdmin can check all.
        /// </summary>
        [HttpGet("/api/attendance/status")]
        public async Task<IActionResult> GetAttendanceStatus(
            [FromQuery] int userId,
            [FromQuery] DateTime date)
        {
            try
            {
                // Validate parameters
                if (userId <= 0)
                {
                    return BadRequest(new AttendanceStatusResponse
                    {
                        Success = false,
                        Message = "UserId is required.",
                        Data = null
                    });
                }

                if (date == default(DateTime))
                {
                    return BadRequest(new AttendanceStatusResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.DateRequired,
                        Data = null
                    });
                }

                // Enforce: users can only check their own attendance (unless HR/TenantAdmin)
                if (!HasElevatedAccess && userId != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UserAttemptedAccessAttendance,
                        CurrentUserId, userId, userId);
                    return UserAccessDenied();
                }

                var tenantId = CurrentOrganisationId;

                Logger.LogInformation(LogMessages.Attendance.GettingAttendanceStatus, userId, date);
                var result = await _service.GetAttendanceStatusAsync(userId, date, tenantId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.GetStatus, nameof(GetAttendanceStatus), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        /// <summary>
        /// Get attendance overview for an employee
        /// GET: attendance/overview?userId=10&tenantId=1&fromDate=2025-01-01&toDate=2025-01-31
        /// Note: Regular users can only see their own attendance. HR/TenantAdmin can see all.
        /// </summary>
        [HttpGet("overview")]
        public async Task<IActionResult> GetAttendanceOverview(
            [FromQuery] int userId,
            [FromQuery] int organisationId,
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            try
            {
                var validatedTenantIdForAccess = GetValidatedOrganisationId(organisationId);

                // Validate parameters
                if (userId <= 0)
                {
                    return BadRequest(new AttendanceOverviewResponse
                    {
                        Success = false,
                        Message = "UserId is required.",
                        Data = null
                    });
                }

                if (organisationId <= 0)
                {
                    return BadRequest(new AttendanceOverviewResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.OrganisationIdRequired,
                        Data = null
                    });
                }

                if (fromDate == default(DateTime) || toDate == default(DateTime))
                {
                    return BadRequest(new AttendanceOverviewResponse
                    {
                        Success = false,
                        Message = "FromDate and ToDate are required",
                        Data = null
                    });
                }

                if (fromDate > toDate)
                {
                    return BadRequest(new AttendanceOverviewResponse
                    {
                        Success = false,
                        Message = AttendanceMessages.InvalidDateRange,
                        Data = null
                    });
                }

                // Validate tenant access - user can only access their own organisation's data
                var validatedTenantId = validatedTenantIdForAccess;

                // Enforce: users can only see their own attendance (unless HR/TenantAdmin)
                if (!HasElevatedAccess && userId != CurrentUserId)
                {
                    Logger.LogWarning(LogMessages.TenantAccess.UserAttemptedAccessAttendance,
                        CurrentUserId, userId, userId);
                    return UserAccessDenied();
                }

                Logger.LogInformation(LogMessages.Attendance.GettingAttendanceOverview,
                    userId, validatedTenantId, fromDate, toDate);

                var request = new AttendanceOverviewRequest
                {
                    UserId = userId,
                    organisationId = validatedTenantId,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                var result = await _attendanceOverviewService.GetAttendanceOverviewAsync(request);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.AttendanceOverview.GetOverview, nameof(GetAttendanceOverview), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }

        /// <summary>
        /// Get today's punch in / punch out logs for the logged-in user
        /// GET: /api/attendance/today-logs
        /// </summary>
        [HttpGet("/api/attendance/today-logs")]
        public async Task<IActionResult> GetTodayPunchLogs()
        {
            try
            {
                var userId = CurrentUserId;
                if (!userId.HasValue)
                {
                    return Unauthorized(new
                    {
                        Success = false,
                        Message = TenantAccessMessages.UserNotAuthenticated
                    });
                }

                var tenantId = CurrentOrganisationId;

                Logger.LogInformation(LogMessages.Attendance.FetchingTodayPunchLogs, userId.Value);

                var result = await _service.GetTodayPunchLogsAsync(userId.Value, tenantId);

                if (result.Success)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Logger.LogException(ExceptionCodes.Attendance.GetTodayPunchLogs, nameof(GetTodayPunchLogs), ex, CurrentUserId);
                return StatusCode(500, new { Success = false, Message = GeneralMessages.SomethingWentWrongContactAdmin });
            }
        }
    }
}
