using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Repositories;
using MobileWebApi.Interfaces;
using MobileWebApi.Constants;
using System.Threading.Tasks;

namespace MobileWebApi.Controllers
{
    [ApiController]
    [Authorize]
   // [Route("locations")]
    public class LocationsController : TenantBaseController
    {
        private readonly LocationRepository _locationRepository;

        public LocationsController(
            LocationRepository locationRepository, 
            ITenantContext tenantContext,
            ILogger<LocationsController> logger)
            : base(tenantContext, logger)
        {
            _locationRepository = locationRepository;
        }

        /// <summary>
        /// Get locations by filters
        /// Note: organization_id is validated against user's tenant for security.
        /// </summary>
        //[HttpGet("get-locations-by-id")]
        //public async Task<IActionResult> GetLocationsById(
        //    [FromQuery] int? user_id,
        //    [FromQuery] int? organization_id,
        //    [FromQuery] int? branchId)
        //{
        //    // Validate tenant access - use user's org if not specified
        //    var validatedOrgId = GetValidatedOrganisationId(organization_id);
            
        //    Logger.LogInformation(LogMessages.Location.FetchingLocations, 
        //        user_id, validatedOrgId, branchId);

        //    var result = await _locationRepository.GetLocationsAsync(user_id, validatedOrgId, branchId);

        //    if (result == null)
        //    {
        //        Logger.LogWarning(LocationMessages.NoLocationsFound);
        //        return NotFound(new { message = LocationMessages.NoLocationsFound });
        //    }

        //    return Ok(result);
        //}
    }
}
