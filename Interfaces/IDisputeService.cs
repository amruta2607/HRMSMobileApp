using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IDisputeService
    {
        Task<DisputeCategoryResponse> GetDisputeCategoriesAsync();

        /// <summary>
        /// Submits a dispute for the authenticated tenant.
        /// </summary>
        Task<DisputeSubmitResponse> SubmitDisputeAsync(DisputeSubmitRequest request, int tenantId);
    }
}
