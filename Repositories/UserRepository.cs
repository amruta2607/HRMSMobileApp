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

            try
            {
                string query = _queryProvider.Get("GetUserByUsernameOrMobile");

                using var connection = _context.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<User>(
                    query,
                    new { Login = login }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUserByUsernameOrMobileAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserGetUserByUsernameOrMobileDatabaseError}: Failed to fetch user by username or mobile",
                    ex);
            }
        }

        public async Task<User?> GetUserByUsernameForWebLoginAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            try
            {
                string query = _queryProvider.Get("GetUserByUsernameForWebLogin");

                using var connection = _context.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<User>(
                    query,
                    new { Username = username }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUserByUsernameForWebLoginAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserGetUserByUsernameForWebLoginDatabaseError}: Failed to fetch user by username for web login",
                    ex);
            }
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            try
            {
                string query = _queryProvider.Get("GetUserByEmail");

                if (string.IsNullOrWhiteSpace(query))
                    throw new InvalidOperationException("SQL query 'GetUserByEmail' not found.");

                using var connection = _context.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<User>(
                    query,
                    new { Email = email.Trim() }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUserByEmailAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserGetUserByEmailDatabaseError}: Failed to fetch user by email",
                    ex);
            }
        }

		//public async Task<User?> GetUserByEmailAsync(string email)
		//      {
		//          if (string.IsNullOrWhiteSpace(email))
		//              return null;

		//          string query = _queryProvider.Get("GetUserByEmail");

		//          using var connection = _context.CreateConnection();
		//          return await connection.QueryFirstOrDefaultAsync<User>(
		//              query,
		//              new { Email = email }
		//          );
		//      }

        public async Task<User?> GetUserByMobileAsync(string mobileNumber)
        {
            if (string.IsNullOrWhiteSpace(mobileNumber))
                return null;

            try
            {
                string query = _queryProvider.Get("GetUserByMobile");

                using var connection = _context.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<User>(
                    query,
                    new { MobileNumber = mobileNumber }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUserByMobileAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserGetUserByMobileDatabaseError}: Failed to fetch user by mobile",
                    ex);
            }
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
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserGetAllUsersByOrganisationDatabaseError}: Failed to fetch users by organisation id",
                    ex);
            }
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            try
            {
                using var connection = _context.CreateConnection();

                string query = _queryProvider.Get("GetUserById");

                return await connection.QueryFirstOrDefaultAsync<User>(query, new { UserId = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(GetUserByIdAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserGetUserByIdDatabaseError}: Failed to fetch user by id",
                    ex);
            }
        }

        public async Task<int> CreateUserAsync(UserCreateRequest request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(CreateUserAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserCreateUserDatabaseError}: Failed to create user",
                    ex);
            }
        }

        public async Task<bool> UpdateUserAsync(UserUpdateRequest request)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdateUserAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserUpdateUserDatabaseError}: Failed to update user",
                    ex);
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            try
            {
                using var connection = _context.CreateConnection();

                var rowsAffected = await connection.ExecuteAsync(
                    _queryProvider.Get("DeleteUser"),
                    new { UserId = userId });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(DeleteUserAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserDeleteUserDatabaseError}: Failed to delete user",
                    ex);
            }
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            try
            {
                using var connection = _context.CreateConnection();

                var rowsAffected = await connection.ExecuteAsync(
                    _queryProvider.Get("DeactivateUser"),
                    new { UserId = userId });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(DeactivateUserAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserDeactivateUserDatabaseError}: Failed to deactivate user",
                    ex);
            }
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string passwordHash, string passwordSalt)
        {
            try
            {
                using var connection = _context.CreateConnection();

                var rowsAffected = await connection.ExecuteAsync(
                    _queryProvider.Get("UpdatePassword"),
                    new { UserId = userId, PasswordHash = passwordHash, PasswordSalt = passwordSalt });

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error occurred in {Method}", nameof(UpdatePasswordAsync));
                throw new Exception(
                    $"{ExceptionCodes.Repository.UserUpdatePasswordDatabaseError}: Failed to update password",
                    ex);
            }
        }
    }
}
