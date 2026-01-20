using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repo, IEmployeeRepository employeeRepository, ILogger<UserService> logger)
        {
            _repo = repo;
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<UserServiceResponse> GetUserByIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.User.RetrievingUserById, userId);
                var user = await _repo.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning(LogMessages.User.UserNotFound, userId);
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.UserNotFound,
                        Data = null
                    };
                }

                return new UserServiceResponse
                {
                    Success = true,
                    Message = UserMessages.UserRetrievedSuccessfully,
                    Data = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorRetrievingUser, userId);
                return new UserServiceResponse
                {
                    Success = false,
                    Message = string.Format(UserMessages.ErrorRetrievingUser, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<UserServiceResponse> GetUserByUsernameOrMobileAsync(string login)
        {
            try
            {
                _logger.LogInformation(LogMessages.User.RetrievingUserByLogin, login);
                var user = await _repo.GetUserByUsernameOrMobileAsync(login);
                if (user == null)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.UserNotFound,
                        Data = null
                    };
                }

                return new UserServiceResponse
                {
                    Success = true,
                    Message = UserMessages.UserRetrievedSuccessfully,
                    Data = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorRetrievingUserByLogin, login);
                return new UserServiceResponse
                {
                    Success = false,
                    Message = string.Format(UserMessages.ErrorRetrievingUser, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<UserListResponse> GetAllUsersAsync(int organisationId)
        {
            try
            {
                _logger.LogInformation(LogMessages.User.RetrievingUsersForTenant, organisationId);
                var users = await _repo.GetAllAsync(organisationId);
                var userList = users.ToList();

                return new UserListResponse
                {
                    Success = true,
                    Message = UserMessages.UsersRetrievedSuccessfully,
                    Data = userList,
                    TotalRecords = userList.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorRetrievingUsers, organisationId);
                return new UserListResponse
                {
                    Success = false,
                    Message = string.Format(UserMessages.ErrorRetrievingUser, ex.Message),
                    Data = null,
                    TotalRecords = 0
                };
            }
        }

        public async Task<UserServiceResponse> CreateUserAsync(UserCreateRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.User.CreatingUser, request.Username);
                
                // Organization ID is now passed directly as int
                
                // Check if username already exists
                var existingUser = await _repo.GetUserByUsernameOrMobileAsync(request.Username);
                if (existingUser != null)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.UsernameAlreadyExists,
                        Data = null
                    };
                }

                // Check if mobile number already exists
                if (!string.IsNullOrEmpty(request.MobileNumber))
                {
                    var existingMobile = await _repo.GetUserByUsernameOrMobileAsync(request.MobileNumber);
                    if (existingMobile != null)
                    {
                        return new UserServiceResponse
                        {
                            Success = false,
                            Message = UserMessages.MobileNumberAlreadyExists,
                            Data = null
                        };
                    }
                }

                var userId = await _repo.CreateUserAsync(request);
                if (userId <= 0)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.FailedToCreateUser,
                        Data = null
                    };
                }

                var user = await _repo.GetUserByIdAsync(userId);
                return new UserServiceResponse
                {
                    Success = true,
                    Message = UserMessages.UserCreatedSuccessfully,
                    Data = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorCreatingUser);
                return new UserServiceResponse
                {
                    Success = false,
                    Message = string.Format(UserMessages.ErrorCreatingUser, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<UserServiceResponse> UpdateUserAsync(UserUpdateRequest request)
        {
            try
            {
                _logger.LogInformation(LogMessages.User.UpdatingUser, request.UserId);
                
                var existingUser = await _repo.GetUserByIdAsync(request.UserId);
                if (existingUser == null)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.UserNotFound,
                        Data = null
                    };
                }

                var success = await _repo.UpdateUserAsync(request);
                if (!success)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.FailedToUpdateUser,
                        Data = null
                    };
                }

                var user = await _repo.GetUserByIdAsync(request.UserId);
                return new UserServiceResponse
                {
                    Success = true,
                    Message = UserMessages.UserUpdatedSuccessfully,
                    Data = user
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorUpdatingUser, request.UserId);
                return new UserServiceResponse
                {
                    Success = false,
                    Message = string.Format(UserMessages.ErrorUpdatingUser, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<UserServiceResponse> DeleteUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.User.DeletingUser, userId);
                
                var existingUser = await _repo.GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.UserNotFound,
                        Data = null
                    };
                }

                var success = await _repo.DeleteUserAsync(userId);
                if (!success)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.FailedToDeleteUser,
                        Data = null
                    };
                }

                return new UserServiceResponse
                {
                    Success = true,
                    Message = UserMessages.UserDeletedSuccessfully,
                    Data = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorDeletingUser, userId);
                return new UserServiceResponse
                {
                    Success = false,
                    Message = string.Format(UserMessages.ErrorDeletingUser, ex.Message),
                    Data = null
                };
            }
        }

        public async Task<UserServiceResponse> DeactivateUserAsync(int userId)
        {
            try
            {
                _logger.LogInformation(LogMessages.User.DeactivatingUser, userId);
                
                var existingUser = await _repo.GetUserByIdAsync(userId);
                if (existingUser == null)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.UserNotFound,
                        Data = null
                    };
                }

                // Check if user is already inactive
                if (!existingUser.IsActive)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.UserAlreadyInactive,
                        Data = null
                    };
                }

                var success = await _repo.DeactivateUserAsync(userId);
                if (!success)
                {
                    return new UserServiceResponse
                    {
                        Success = false,
                        Message = UserMessages.FailedToDeactivateUser,
                        Data = null
                    };
                }

                // Fetch updated user to return
                var updatedUser = await _repo.GetUserByIdAsync(userId);
                return new UserServiceResponse
                {
                    Success = true,
                    Message = UserMessages.UserDeactivatedSuccessfully,
                    Data = updatedUser
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, LogMessages.User.ErrorDeactivatingUser, userId);
                return new UserServiceResponse
                {
                    Success = false,
                    Message = string.Format(UserMessages.ErrorDeactivatingUser, ex.Message),
                    Data = null
                };
            }
        }
    }
}
