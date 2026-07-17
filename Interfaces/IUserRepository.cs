using MobileWebApi.Models;

namespace MobileWebApi.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync(int tenantId);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameOrMobileAsync(string login);
        Task<User?> GetUserByUsernameForWebLoginAsync(string username);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByMobileAsync(string mobileNumber);
        Task<IReadOnlyList<string>> GetActiveWorkRolesByUserIdAsync(int userId);
        Task<int> CreateUserAsync(UserCreateRequest request);
        Task<bool> UpdateUserAsync(UserUpdateRequest request);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        Task<bool> UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt);
    }
}
