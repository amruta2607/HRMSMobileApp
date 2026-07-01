using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingService
    {
        Task<LocationTrackingResponse> RecordLocationAsync(LocationTrackingRequest request, int currentUserId, int organisationId);
    }
}
