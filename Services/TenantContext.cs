using MobileWebApi.Interfaces;
using MobileWebApi.Constants;
using System.Security.Claims;

namespace MobileWebApi.Services
{
    /// <summary>
    /// Custom exception thrown when a user attempts to access another tenant's data.
    /// </summary>
    public class TenantAccessException : UnauthorizedAccessException
    {
        public int? RequestedOrganisationId { get; }
        public int? UserOrganisationId { get; }

        public TenantAccessException(int? requestedOrgId, int? userOrgId)
            : base($"Access denied: User from organisation {userOrgId} cannot access data from organisation {requestedOrgId}")
        {
            RequestedOrganisationId = requestedOrgId;
            UserOrganisationId = userOrgId;
        }

        public TenantAccessException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Provides tenant context from the current HTTP request's authenticated user.
    /// Extracts organisation information from JWT claims to enforce tenant isolation.
    /// </summary>
    public class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<TenantContext> _logger;

        public TenantContext(IHttpContextAccessor httpContextAccessor, ILogger<TenantContext> logger)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public int? OrganisationId
        {
            get
            {
                var claim = User?.FindFirst("OrganisationId")?.Value;
                if (int.TryParse(claim, out int orgId))
                    return orgId;
                return null;
            }
        }

        public int? BranchId
        {
            get
            {
                var claim = User?.FindFirst("BranchId")?.Value;
                if (int.TryParse(claim, out int branchId) && branchId > 0)
                    return branchId;
                return null;
            }
        }

        public int? UserId
        {
            get
            {
                var claim = User?.FindFirst("UserId")?.Value;
                if (int.TryParse(claim, out int userId))
                    return userId;
                return null;
            }
        }

        public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value;

        public bool IsHrUser
        {
            get
            {
                var claim = User?.FindFirst("IsHrUser")?.Value;
                return bool.TryParse(claim, out bool isHr) && isHr;
            }
        }

        public bool IsTenantAdmin
        {
            get
            {
                var claim = User?.FindFirst("IsTenantAdmin")?.Value;
                return bool.TryParse(claim, out bool isAdmin) && isAdmin;
            }
        }

        public bool HasElevatedAccess => IsHrUser || IsTenantAdmin;

        public int GetRequiredOrganisationId()
        {
            var orgId = OrganisationId;
            if (!orgId.HasValue)
            {
                _logger.LogWarning(LogMessages.TenantContext.AttemptedToGetOrganisationIdNotAuthenticated, Username ?? "Unknown");
                throw new TenantAccessException("User is not authenticated or organisation information is missing");
            }
            return orgId.Value;
        }

        public int GetRequiredBranchId()
        {
            var branchId = BranchId;
            if (!branchId.HasValue)
            {
                _logger.LogWarning(LogMessages.TenantContext.AttemptedToGetBranchIdNotAuthenticated, Username ?? "Unknown");
                throw new TenantAccessException("User is not authenticated or branch information is missing");
            }
            return branchId.Value;
        }

        public void ValidateTenantAccess(int requestedOrganisationId)
        {
            var userOrgId = OrganisationId;

            if (!userOrgId.HasValue)
            {
                _logger.LogWarning(LogMessages.TenantContext.TenantAccessValidationFailedNotAuthenticated, requestedOrganisationId);
                throw new TenantAccessException("User is not authenticated");
            }

            if (userOrgId.Value != requestedOrganisationId)
            {
                _logger.LogWarning(
                    LogMessages.TenantContext.TenantAccessViolationDetected,
                    Username ?? "Unknown",
                    userOrgId.Value,
                    requestedOrganisationId);

                throw new TenantAccessException(requestedOrganisationId, userOrgId.Value);
            }

            _logger.LogDebug(LogMessages.TenantContext.TenantAccessValidated, Username, userOrgId.Value);
        }

        public int GetRequiredUserId()
        {
            var userId = UserId;
            if (!userId.HasValue)
            {
                _logger.LogWarning(LogMessages.TenantContext.AttemptedToGetUserIdNotAuthenticated, Username ?? "Unknown");
                throw new TenantAccessException("User is not authenticated or user ID is missing");
            }
            return userId.Value;
        }

        public bool CanAccessUser(int requestedUserId)
        {
            var currentUserId = UserId;
            
            // Not authenticated
            if (!currentUserId.HasValue)
                return false;
            
            // HR or TenantAdmin can access any user's data
            if (HasElevatedAccess)
                return true;
            
            // Regular users can only access their own data
            return currentUserId.Value == requestedUserId;
        }

        public void ValidateUserAccess(int requestedUserId)
        {
            var currentUserId = UserId;

            if (!currentUserId.HasValue)
            {
                _logger.LogWarning(LogMessages.TenantContext.UserAccessValidationFailedNotAuthenticated, requestedUserId);
                throw new TenantAccessException("User is not authenticated");
            }

            // HR or TenantAdmin can access any user's data within their organisation
            if (HasElevatedAccess)
            {
                _logger.LogDebug(LogMessages.TenantContext.UserHasElevatedAccess, Username, requestedUserId);
                return;
            }

            // Regular users can only access their own data
            if (currentUserId.Value != requestedUserId)
            {
                _logger.LogWarning(
                    LogMessages.TenantContext.UserAccessViolationDetected,
                    Username ?? "Unknown",
                    currentUserId.Value,
                    requestedUserId);

                throw new TenantAccessException($"Access denied: You can only access your own data. Your UserId: {currentUserId.Value}, Requested UserId: {requestedUserId}");
            }

            _logger.LogDebug(LogMessages.TenantContext.UserAccessValidated, Username, currentUserId.Value);
        }
    }
}

