using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IDisputeService
    {
        Task<DisputeCategoryResponse> GetDisputeCategoriesAsync();
        Task<DisputeSubmitResponse> SubmitDisputeAsync(DisputeSubmitRequest request);
    }
}

