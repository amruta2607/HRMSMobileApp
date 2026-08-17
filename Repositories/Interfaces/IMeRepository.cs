using MobileWebApi.Models.Responses;

namespace MobileWebApi.Repositories.Interfaces
{
    /// <summary>
    /// Loads the currently authenticated user's profile and work roles.
    /// </summary>
    public interface IMeRepository
    {
        /// <summary>
        /// Returns the current user profile, or null when the user does not exist.
        /// </summary>
        Task<MeResponse?> GetCurrentUserAsync();
    }
}
