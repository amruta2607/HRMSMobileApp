using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Models;
using MobileWebApi.Constants;

namespace MobileWebApi.Controllers
{
    [ApiController]
    [Route("user")]
    [Authorize]
    public class UserController : TenantBaseController
    {
        private readonly IUserService _userService;

        public UserController(
            IUserService userService, 
            ITenantContext tenantContext,
            ILogger<UserController> logger) 
            : base(tenantContext, logger)
        {
            _userService = userService;
        }

        /// <summary>
        /// Get user by ID
        /// GET: user/get-user-id/?id=7
        /// Note: Regular users can only access their own data. HR/TenantAdmin can access all users.
        /// </summary>
        [HttpGet("get-user-id")]
        public async Task<IActionResult> GetUserById([FromQuery] int id)
        {
            if (id <= 0)
            {
                Logger.LogWarning(UserMessages.InvalidUserId);
                return BadRequest(new { Success = false, Message = UserMessages.InvalidUserId });
            }

            // Validate user access - regular users can only access their own data
            try
            {
                var validatedId = GetValidatedUserId(id);
                id = validatedId;
            }
            catch (Services.TenantAccessException)
            {
                return UserAccessDenied();
            }

            Logger.LogInformation(LogMessages.User.RetrievingUserById, id);
            var result = await _userService.GetUserByIdAsync(id);

            if (!result.Success)
            {
                Logger.LogWarning(LogMessages.User.UserNotFound, id);
                return NotFound(result);
            }

            // Validate tenant access - ensure fetched user belongs to same organisation
            if (result.Data != null)
            {
                TenantContext.ValidateTenantAccess(result.Data.OrganisationId);
            }

            return Ok(result);
        }

        /// <summary>
        /// Delete user by ID
        /// GET: user/delete-user/?id=6
        /// Note: Users can only delete users from their own organisation.
        /// </summary>
        [HttpGet("delete-user")]
        public async Task<IActionResult> DeleteUser([FromQuery] int id)
        {
            if (id <= 0)
            {
                Logger.LogWarning(UserMessages.InvalidUserId);
                return BadRequest(new { Success = false, Message = UserMessages.InvalidUserId });
            }

            // First fetch the user to validate tenant access
            var userResult = await _userService.GetUserByIdAsync(id);
            if (!userResult.Success || userResult.Data == null)
            {
                Logger.LogWarning(LogMessages.User.UserNotFound, id);
                return NotFound(userResult);
            }

            // Validate tenant access - ensure user belongs to same organisation
            TenantContext.ValidateTenantAccess(userResult.Data.OrganisationId);

            Logger.LogInformation(LogMessages.User.DeletingUserHard, id);
            var result = await _userService.DeleteUserAsync(id);

            if (!result.Success)
            {
                Logger.LogWarning(LogMessages.User.ErrorDeletingUser, id);
                return NotFound(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Add new user
        /// POST: apipunch/user/add-user
        /// Note: Users can only create users in their own organisation.
        /// </summary>
        //[HttpPost("/apipunch/user/add-user")]
        //public async Task<IActionResult> AddUser([FromBody] UserCreateRequest request)
        //{
        //    if (string.IsNullOrEmpty(request.Username))
        //    {
        //        Logger.LogWarning(UserMessages.UsernameRequired);
        //        return BadRequest(new { Success = false, Message = UserMessages.UsernameRequired });
        //    }

        //    if (string.IsNullOrEmpty(request.Password))
        //    {
        //        Logger.LogWarning(UserMessages.PasswordRequired);
        //        return BadRequest(new { Success = false, Message = UserMessages.PasswordRequired });
        //    }

        //    // Enforce tenant isolation: use validated organisation ID
        //    // If organization provided, validate it matches user's org; otherwise use user's org
        //    request.organization = GetValidatedOrganisationId(request.organization > 0 ? request.organization : null);

        //    Logger.LogInformation(LogMessages.User.AddingUser, request.Username);
        //    var result = await _userService.CreateUserAsync(request);

        //    if (!result.Success)
        //    {
        //        Logger.LogWarning(LogMessages.User.ErrorCreatingUser);
        //        return BadRequest(result);
        //    }

        //    return Ok(result);
        //}

        /// <summary>
        /// Deactivate/Inactivate a user (set IsActive = 0)
        /// PUT: /apipunch/user/inactive-user/
        /// Body: { "user_id": 111 }
        /// </summary>
        [HttpPut("/apipunch/user/inactive-user")]
        public async Task<IActionResult> InactiveUser([FromBody] UserInactiveRequest request)
        {
            if (request == null)
            {
                Logger.LogWarning(GeneralMessages.RequestBodyCannotBeNull);
                return BadRequest(new { Success = false, Message = GeneralMessages.RequestBodyCannotBeNull });
            }

            if (request.user_id <= 0)
            {
                Logger.LogWarning(UserMessages.InvalidUserId);
                return BadRequest(new { Success = false, Message = UserMessages.InvalidUserId });
            }

            // First fetch the user to validate tenant access
            var userResult = await _userService.GetUserByIdAsync(request.user_id);
            if (!userResult.Success || userResult.Data == null)
            {
                Logger.LogWarning(LogMessages.User.UserNotFound, request.user_id);
                return NotFound(userResult);
            }

            // Validate tenant access - ensure user belongs to same organisation
            TenantContext.ValidateTenantAccess(userResult.Data.OrganisationId);

            Logger.LogInformation(LogMessages.User.DeactivatingUser, request.user_id);
            var result = await _userService.DeactivateUserAsync(request.user_id);

            if (!result.Success)
            {
                Logger.LogWarning(LogMessages.User.ErrorDeactivatingUser, request.user_id);
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
