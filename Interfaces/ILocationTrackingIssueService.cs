using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingIssueService
    {
        Task<LocationTrackingResponse> AddLocationTrackingIssueAsync(
            LocationTrackingIssueRequest request,
            int currentUserId,
            int organisationId);
    }
}
