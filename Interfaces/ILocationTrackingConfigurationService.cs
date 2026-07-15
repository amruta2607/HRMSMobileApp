using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingConfigurationService
    {
        Task<(bool Success, string Message, LocationTrackingConfigurationResponse? Data)> GetConfigurationAsync(
            int userId,
            int organisationId);
    }
}
