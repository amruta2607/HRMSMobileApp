using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IHolidayRepository
    {
        /// <summary>
        /// Create a new holiday
        /// </summary>
        Task<int> CreateHolidayAsync(Holiday holiday);
        
        /// <summary>
        /// Get holiday by ID
        /// </summary>
        Task<Holiday?> GetHolidayByIdAsync(int id, int tenantId);
        
        /// <summary>
        /// Get all holidays for a tenant
        /// </summary>
        Task<IEnumerable<Holiday>> GetAllHolidaysAsync(int tenantId);
        
        /// <summary>
        /// Get holidays with filters (year)
        /// </summary>
        Task<IEnumerable<Holiday>> GetHolidaysWithFiltersAsync(int tenantId, int? year);
        
        /// <summary>
        /// Update a holiday
        /// </summary>
        Task<bool> UpdateHolidayAsync(Holiday holiday);
        
        /// <summary>
        /// Delete a holiday
        /// </summary>
        Task<bool> DeleteHolidayAsync(int id, int tenantId);
        
        /// <summary>
        /// Create multiple holidays in bulk
        /// </summary>
        Task<int> BulkCreateHolidaysAsync(IEnumerable<Holiday> holidays);
    }
}

