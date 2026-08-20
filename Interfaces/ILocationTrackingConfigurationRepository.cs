using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingConfigurationRepository
    {
        Task<LocationTrackingConfiguration?> GetByIdAsync(int id);
    }
}
