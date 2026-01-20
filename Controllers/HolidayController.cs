using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HolidayController : TenantBaseController
    {
        private readonly IHolidayService _holidayService;

        public HolidayController(
            IHolidayService holidayService,
            ITenantContext tenantContext,
            ILogger<HolidayController> logger)
            : base(tenantContext, logger)
        {
            _holidayService = holidayService;
        }

        /// <summary>
        /// Add a new holiday
        /// POST: api/holiday
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddHoliday([FromBody] HolidayCreateRequest request)
        {
            if (request == null)
            {
                Logger.LogWarning(HolidayMessages.HolidayNameRequired);
                return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
            }

            var tenantId = CurrentOrganisationId;
            var userId = CurrentUserId ?? 0;

            Logger.LogInformation(LogMessages.Holiday.CreatingHoliday, request.holiday_name);
            var result = await _holidayService.AddHolidayAsync(request, tenantId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get holidays with filters (OrdiNet compatible)
        /// GET: /apipunch/holidays/get-holidays/?year=2025&user_id=9
        /// </summary>
        [HttpGet("/apipunch/holidays/get-holidays")]
        public async Task<IActionResult> GetHolidays(
            [FromQuery] int? user_id = null,
            [FromQuery] int? organization_id = null,
            [FromQuery] int? year = null)
        {
            // If organization_id is provided, validate tenant access
            // Otherwise, let the service determine it from user_id
            int? validatedOrgId = null;
            if (organization_id.HasValue)
            {
                validatedOrgId = GetValidatedOrganisationId(organization_id);
            }

            Logger.LogInformation(LogMessages.Holiday.FetchingHolidays, validatedOrgId ?? 0);
            var result = await _holidayService.GetHolidaysWithFiltersAsync(user_id, validatedOrgId, year);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get all holidays for the tenant
        /// GET: api/holiday
        /// </summary>
        //[HttpGet]
        //public async Task<IActionResult> GetAllHolidays()
        //{
        //    var tenantId = CurrentOrganisationId;

        //    Logger.LogInformation(LogMessages.Holiday.FetchingHolidays, tenantId);
        //    var result = await _holidayService.GetAllHolidaysAsync(tenantId);

        //    if (result.Success)
        //    {
        //        return Ok(result);
        //    }

        //    return BadRequest(result);
        //}

        /// <summary>
        /// Update a holiday
        /// PUT: api/holiday/update-holiday
        /// </summary>
        [HttpPut("update-holiday")]
        public async Task<IActionResult> UpdateHoliday([FromBody] HolidayUpdateRequest request)
        {
            if (request == null)
            {
                Logger.LogWarning(GeneralMessages.RequestBodyCannotBeNull);
                return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
            }

            if (request.Id <= 0)
            {
                Logger.LogWarning("Invalid holiday ID");
                return BadRequest(new { Success = false, Message = HolidayMessages.InvalidHolidayId });
            }

            var tenantId = CurrentOrganisationId;
            var userId = CurrentUserId ?? 0;

            Logger.LogInformation(LogMessages.Holiday.UpdatingHoliday, request.Id);
            var result = await _holidayService.UpdateHolidayAsync(request, tenantId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Delete a holiday
        /// DELETE: api/holiday/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHoliday(int id)
        {
            var tenantId = CurrentOrganisationId;

            Logger.LogInformation(LogMessages.Holiday.DeletingHoliday, id);
            var result = await _holidayService.DeleteHolidayAsync(id, tenantId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Add multiple holidays in bulk
        /// POST: api/holiday/bulk
        /// </summary>
        [HttpPost("bulk")]
        public async Task<IActionResult> AddBulkHolidays([FromBody] HolidayBulkCreateRequest request)
        {
            if (request == null || request.Holidays == null || !request.Holidays.Any())
            {
                Logger.LogWarning(HolidayMessages.HolidaysListRequired);
                return BadRequest(new { Success = false, Message = HolidayMessages.HolidaysListRequired });
            }

            var tenantId = CurrentOrganisationId;
            var userId = CurrentUserId ?? 0;

            Logger.LogInformation(LogMessages.Holiday.CreatingBulkHolidays, request.Holidays.Count);
            var result = await _holidayService.AddBulkHolidaysAsync(request, tenantId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Update holiday date (form data)
        /// POST: /holidays/update-holidays/
        /// Accepts form data with: id and date
        /// </summary>
        //[HttpPost("/holidays/update-holidays")]
        //public async Task<IActionResult> UpdateHolidayDate([FromForm] HolidayUpdateDateRequest request)
        //{
        //    if (request == null)
        //    {
        //        Logger.LogWarning(GeneralMessages.RequestBodyCannotBeNull);
        //        return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
        //    }

        //    if (request.id <= 0)
        //    {
        //        Logger.LogWarning(HolidayMessages.InvalidHolidayId);
        //        return BadRequest(new { Success = false, Message = HolidayMessages.InvalidHolidayId });
        //    }

        //    if (request.date == default(DateTime))
        //    {
        //        Logger.LogWarning(HolidayMessages.HolidayDateRequired);
        //        return BadRequest(new { Success = false, Message = HolidayMessages.HolidayDateRequired });
        //    }

        //    var tenantId = CurrentOrganisationId;
        //    var userId = CurrentUserId ?? 0;

        //    Logger.LogInformation(LogMessages.Holiday.UpdatingHoliday, request.id);
        //    var result = await _holidayService.UpdateHolidayDateAsync(request.id, request.date, tenantId, userId);

        //    if (result.Success)
        //    {
        //        return Ok(result);
        //    }

        //    return BadRequest(result);
        //}
    }
}

