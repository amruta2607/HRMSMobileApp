namespace MobileWebApi.Interfaces
{
    /// <summary>
    /// Provides tenant context information from the current authenticated user.
    /// Used to enforce tenant isolation across the application.
    /// </summary>
    public interface ITenantContext
    {
        /// <summary>
        /// Gets the current user's organisation/tenant ID from JWT claims.
        /// Returns null if user is not authenticated or claim is missing.
        /// </summary>
        int? OrganisationId { get; }

        /// <summary>
        /// Gets the current user's assigned branch ID from JWT claims.
        /// Returns null if user is not authenticated or claim is missing.
        /// </summary>
        int? BranchId { get; }

        /// <summary>
        /// Gets the current user's ID from JWT claims.
        /// </summary>
        int? UserId { get; }

        /// <summary>
        /// Gets the current user's username from JWT claims.
        /// </summary>
        string? Username { get; }

        /// <summary>
        /// Checks if the user is authenticated and has a valid tenant context.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Checks if the current user is an HR user.
        /// HR users can access all users' data within their organisation.
        /// </summary>
        bool IsHrUser { get; }

        /// <summary>
        /// Checks if the current user is a Tenant Admin.
        /// Tenant Admins can access all users' data within their organisation.
        /// </summary>
        bool IsTenantAdmin { get; }

        /// <summary>
        /// Checks if the current user has elevated access (HR or TenantAdmin).
        /// Users with elevated access can view all users' data within their organisation.
        /// </summary>
        bool HasElevatedAccess { get; }

        /// <summary>
        /// Validates that the requested organisation ID matches the user's tenant.
        /// Throws TenantAccessException if access is denied.
        /// </summary>
        /// <param name="requestedOrganisationId">The organisation ID being accessed</param>
        void ValidateTenantAccess(int requestedOrganisationId);

        /// <summary>
        /// Validates user-level access. Regular users can only access their own data.
        /// HR and TenantAdmin users can access any user's data within their organisation.
        /// </summary>
        /// <param name="requestedUserId">The user ID being accessed</param>
        void ValidateUserAccess(int requestedUserId);

        /// <summary>
        /// Checks if the current user can access another user's data.
        /// Returns true if user has elevated access or is accessing their own data.
        /// </summary>
        /// <param name="requestedUserId">The user ID being accessed</param>
        bool CanAccessUser(int requestedUserId);

        /// <summary>
        /// Gets the organisation ID, throwing if not available.
        /// Use this when organisation ID is required.
        /// </summary>
        int GetRequiredOrganisationId();

        /// <summary>
        /// Gets the current user's assigned branch ID, throwing if not available.
        /// Use this when branch ID is required.
        /// </summary>
        int GetRequiredBranchId();

        /// <summary>
        /// Gets the current user's ID, throwing if not available.
        /// Use this when user ID is required.
        /// </summary>
        int GetRequiredUserId();
    }
}

