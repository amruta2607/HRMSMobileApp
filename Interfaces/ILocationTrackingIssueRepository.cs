using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface ILocationTrackingIssueRepository
    {
        Task<int> InsertAsync(LocationTrackingIssue issue);
    }
}
