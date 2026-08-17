using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IHolidayService
    {
        /// <summary>
        /// Add a new holiday
        /// </summary>
        Task<HolidayResponse> AddHolidayAsync(HolidayCreateRequest request, int tenantId, int userId);
        
        /// <summary>
        /// Get all holidays for the tenant
        /// </summary>
        Task<HolidayResponse> GetAllHolidaysAsync(int tenantId);
        
        /// <summary>
        /// Get holidays with filters (OrdiNet compatible)
        /// </summary>
        Task<HolidayResponse> GetHolidaysWithFiltersAsync(int? userId, int? organizationId, int? year);
        
        /// <summary>
        /// Update a holiday
        /// </summary>
        Task<HolidayResponse> UpdateHolidayAsync(HolidayUpdateRequest request, int tenantId, int userId);
        
        /// <summary>
        /// Delete a holiday
        /// </summary>
        Task<HolidayResponse> DeleteHolidayAsync(int id, int tenantId);
        
        /// <summary>
        /// Add multiple holidays in bulk
        /// </summary>
        Task<HolidayResponse> AddBulkHolidaysAsync(HolidayBulkCreateRequest request, int tenantId, int userId);
        
        /// <summary>
        /// Update holiday date only (simple form data update)
        /// </summary>
        Task<HolidayResponse> UpdateHolidayDateAsync(int id, DateTime date, int tenantId, int userId);
    }
}

