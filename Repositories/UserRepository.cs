using Dapper;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Helper;
using MobileWebApi.Resources;
using MobileWebApi.Constants;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MobileWebApi.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DapperContext _context;
        private readonly ILogger<UserRepository> _logger;
        private readonly QueryProvider _queryProvider;

        public UserRepository(DapperContext context, ILogger<UserRepository> logger, QueryProvider queryProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _queryProvider = queryProvider;
        }

        public async Task<User?> GetUserByUsernameOrMobileAsync(string login)
        {
            if (string.IsNullOrWhiteSpace(login))
                return null;

            string query = _queryProvider.Get("GetUserByUsernameOrMobile");

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                query,
                new { Login = login }
            );
        }

        public async Task<User?> GetUserByUsernameForWebLoginAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            string query = _queryProvider.Get("GetUserByUsernameForWebLogin");

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                query,
                new { Username = username }
            );
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            string query = _queryProvider.Get("GetUserByEmail");

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                query,
                new { Email = email }
            );
        }

        public async Task<User?> GetUserByMobileAsync(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return null;

            string query = _queryProvider.Get("GetUserByMobile");

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                query,
                new { MobileNumber = mobileNumber }
            );
        }

        public async Task<IEnumerable<User>> GetAllAsync(int organisationId)
        {
            try
            {
                string query = _queryProvider.Get("GetUsersByOrganisationId");

                using var connection = _context.CreateConnection();
                return await connection.QueryAsync<User>(
                    query,
                    new { OrganisationId = organisationId }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorFetchingUsersByOrganisationId);
                throw;
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var connection = _context.CreateConnection();

            string query = _queryProvider.Get("GetUserById");

            return await connection.QueryFirstOrDefaultAsync<User>(query, new { UserId = userId });
        }

        public async Task<int> CreateUserAsync(UserCreateRequest request)
        {
            using var connection = _context.CreateConnection();

            // Convert WorkRoleName → WorkRoleId
            int workRoleId = 0;
            if (!string.IsNullOrEmpty(request.WorkRoleName))
            {
                workRoleId = await connection.QueryFirstOrDefaultAsync<int>(
                    _queryProvider.Get("GetWorkRoleIdByName"),
                    new { WorkRoleName = request.WorkRoleName });
            }

            // Generate password hash and salt
            string salt = PasswordHelper.GenerateSalt();
            string passwordHash = PasswordHelper.HashPassword(request.Password, salt);

            var userId = await connection.QuerySingleAsync<int>(
                _queryProvider.Get("CreateUser"),
                new
                {
                    request.Username,
                    request.DisplayName,
                    request.Email,
                    request.MobileNumber,
                    request.PinNumber,
                    PasswordHash = passwordHash,
                    PasswordSalt = salt,
                    WorkRoleId = workRoleId,
                    OrganisationId = request.organization,
                    BranchId = request.branch,
                    request.IsHrUser,
                    request.IsTenantAdmin,
                    request.IsActive
                });

            return userId;
        }

        public async Task<bool> UpdateUserAsync(UserUpdateRequest request)
        {
            using var connection = _context.CreateConnection();

            // Convert WorkRoleName → WorkRoleId
            int workRoleId = 0;
            if (!string.IsNullOrEmpty(request.WorkRoleName))
            {
                workRoleId = await connection.QueryFirstOrDefaultAsync<int>(
                    _queryProvider.Get("GetWorkRoleIdByName"),
                    new { request.WorkRoleName });

                if (workRoleId <= 0)
                    throw new Exception("Invalid WorkRoleName");
            }

            var rowsAffected = await connection.ExecuteAsync(
                _queryProvider.Get("UpdateUser"),
                new
                {
                    request.UserId,
                    DisplayName = request.DisplayName,
                    EmailId = request.email_id,
                    MobileNumber = request.mobile_number,
                    PinNumber = request.password,
                    WorkRoleId = workRoleId,
                    request.IsActive
                });

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var connection = _context.CreateConnection();

            var rowsAffected = await connection.ExecuteAsync(
                _queryProvider.Get("DeleteUser"),
                new { UserId = userId });

            return rowsAffected > 0;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            using var connection = _context.CreateConnection();

            var rowsAffected = await connection.ExecuteAsync(
                _queryProvider.Get("DeactivateUser"),
                new { UserId = userId });

            return rowsAffected > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt)
        {
            using var connection = _context.CreateConnection();

            var rowsAffected = await connection.ExecuteAsync(
                _queryProvider.Get("UpdatePassword"),
                new { UserId = userId, PasswordHash = passwordHash, PasswordSalt = passwordSalt });

            return rowsAffected > 0;
        }
    }
}
