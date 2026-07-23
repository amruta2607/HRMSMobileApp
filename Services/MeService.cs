using MobileWebApi.Interfaces;
using MobileWebApi.Models.Responses;
using MobileWebApi.Repositories.Interfaces;

namespace MobileWebApi.Services
{
    /// <summary>
    /// Service for the authenticated user's profile (Me API).
    /// </summary>
    public class MeService : IMeService
    {
        private readonly IMeRepository _meRepository;

        public MeService(IMeRepository meRepository)
        {
            _meRepository = meRepository ?? throw new ArgumentNullException(nameof(meRepository));
        }

        /// <inheritdoc />
        public Task<MeResponse?> GetCurrentUserAsync()
        {
            return _meRepository.GetCurrentUserAsync();
        }
    }
}
