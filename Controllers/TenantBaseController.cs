using Microsoft.AspNetCore.Mvc;
using MobileWebApi.Interfaces;
using MobileWebApi.Services;
using MobileWebApi.Constants;

namespace MobileWebApi.Controllers
{
    /// <summary>
    /// Base controller that provides tenant isolation capabilities.
    /// All controllers that need tenant isolation should inherit from this.
    /// </summary>
    public abstract class TenantBaseController : ControllerBase
    {
        protected readonly ITenantContext TenantContext;
        protected readonly ILogger Logger;

        protected TenantBaseController(ITenantContext tenantContext, ILogger logger)
        {
            TenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the current user's organisation ID from the JWT token.
        /// Throws 401 Unauthorized if not authenticated.
        /// </summary>
        protected int CurrentOrganisationId => TenantContext.GetRequiredOrganisationId();

        /// <summary>
        /// Gets the current user's ID from the JWT token.
        /// </summary>
        protected int? CurrentUserId => TenantContext.UserId;

        /// <summary>
        /// Gets the current username from the JWT token.
        /// </summary>
        protected string? CurrentUsername => TenantContext.Username;

        /// <summary>
        /// Checks if the current user is an HR user.
        /// </summary>
        protected bool IsHrUser => TenantContext.IsHrUser;

        /// <summary>
        /// Checks if the current user is a Tenant Admin.
        /// </summary>
        protected bool IsTenantAdmin => TenantContext.IsTenantAdmin;

        /// <summary>
        /// Checks if the current user has elevated access (HR or TenantAdmin).
        /// Users with elevated access can view all users' data within their organisation.
        /// </summary>
        protected bool HasElevatedAccess => TenantContext.HasElevatedAccess;

        /// <summary>
        /// Validates that the requested organisation ID matches the current user's organisation.
        /// If the requestedOrgId is null or 0, returns the user's organisation ID.
        /// Otherwise, validates and returns the requested ID if valid.
        /// </summary>
        /// <param name="requestedOrgId">The organisation ID from the request (query param or body)</param>
        /// <returns>The validated organisation ID to use</returns>
        protected int GetValidatedOrganisationId(int? requestedOrgId)
        {
            var userOrgId = CurrentOrganisationId;

            // If no org ID specified in request, use user's org ID
            if (!requestedOrgId.HasValue || requestedOrgId.Value <= 0)
            {
                return userOrgId;
            }

            // Validate that user can access the requested org
            TenantContext.ValidateTenantAccess(requestedOrgId.Value);
            return requestedOrgId.Value;
        }

        /// <summary>
        /// Gets the validated user ID. 
        /// If the user has elevated access (HR/TenantAdmin), returns the requested user ID.
        /// Otherwise, returns the current user's ID (ignoring the requested ID).
        /// </summary>
        /// <param name="requestedUserId">The user ID from the request</param>
        /// <returns>The user ID to use for data access</returns>
        protected int GetValidatedUserId(int? requestedUserId)
        {
            var currentUserId = CurrentUserId ?? throw new UnauthorizedAccessException(TenantAccessMessages.UserNotAuthenticated);

            // If no user ID specified or user ID is 0, use current user's ID
            if (!requestedUserId.HasValue || requestedUserId.Value <= 0)
            {
                return currentUserId;
            }

            // If user has elevated access, allow access to requested user
            if (HasElevatedAccess)
            {
                return requestedUserId.Value;
            }

            // Regular users can only access their own data
            if (requestedUserId.Value != currentUserId)
            {
                Logger.LogWarning(LogMessages.TenantAccess.UserAccessViolation, 
                    currentUserId, requestedUserId.Value);
                throw new Services.TenantAccessException(TenantAccessMessages.UserAccessDeniedSimple);
            }

            return currentUserId;
        }

        /// <summary>
        /// Checks if the current user can access the requested user's data.
        /// </summary>
        /// <param name="requestedUserId">The user ID being accessed</param>
        /// <returns>True if access is allowed</returns>
        protected bool CanAccessUser(int requestedUserId)
        {
            return TenantContext.CanAccessUser(requestedUserId);
        }

        /// <summary>
        /// Validates that the current user can access the requested user's data.
        /// Throws TenantAccessException if access is denied.
        /// </summary>
        /// <param name="requestedUserId">The user ID being accessed</param>
        protected void ValidateUserAccess(int requestedUserId)
        {
            TenantContext.ValidateUserAccess(requestedUserId);
        }

        /// <summary>
        /// Creates an error response for tenant access violations.
        /// </summary>
        protected IActionResult TenantAccessDenied()
        {
            return StatusCode(403, new
            {
                Success = false,
                Message = TenantAccessMessages.TenantAccessDenied
            });
        }

        /// <summary>
        /// Creates an error response for user access violations.
        /// </summary>
        protected IActionResult UserAccessDenied()
        {
            return StatusCode(403, new
            {
                Success = false,
                Message = TenantAccessMessages.UserAccessDenied
            });
        }

        /// <summary>
        /// Wraps an action with tenant access exception handling.
        /// </summary>
        protected async Task<IActionResult> ExecuteWithTenantValidation(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (TenantAccessException ex)
            {
                Logger.LogWarning(ex, LogMessages.TenantAccess.TenantAccessViolation);
                return TenantAccessDenied();
            }
        }
    }
}

