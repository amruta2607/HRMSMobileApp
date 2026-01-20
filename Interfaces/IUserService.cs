using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IUserService
    {
        Task<UserServiceResponse> GetUserByIdAsync(int userId);
        Task<UserServiceResponse> GetUserByUsernameOrMobileAsync(string login);
        Task<UserListResponse> GetAllUsersAsync(int tenantId);
        Task<UserServiceResponse> CreateUserAsync(UserCreateRequest request);
        Task<UserServiceResponse> UpdateUserAsync(UserUpdateRequest request);
        Task<UserServiceResponse> DeleteUserAsync(int userId);
        Task<UserServiceResponse> DeactivateUserAsync(int userId);
    }
}

