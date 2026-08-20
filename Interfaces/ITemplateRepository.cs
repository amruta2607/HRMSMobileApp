using MobileWebApi.Models.Responses;

namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Provides data access for template lookup operations.
    /// </summary>
    public interface ITemplateRepository
    {
        /// <summary>
        /// Returns all active templates for the authenticated user's tenant.
        /// </summary>
        Task<IEnumerable<TemplateResponse>> GetTemplatesAsync();
    }
}
