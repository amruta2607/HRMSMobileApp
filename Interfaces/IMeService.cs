using MobileWebApi.Models.Responses;

namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Service for the authenticated user's profile (Me API).
    /// </summary>
    public interface IMeService
    {
        /// <summary>
        /// Returns the current user profile, or null when the user does not exist.
        /// </summary>
        Task<MeResponse?> GetCurrentUserAsync();
    }
}
