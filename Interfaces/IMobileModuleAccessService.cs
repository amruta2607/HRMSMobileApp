using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IMobileModuleAccessService
    {
        Task<MobileAccessDto> GetModuleAccess(int tenantId);
        Task<bool> HasAccess(int tenantId, string moduleName);
    }
}

