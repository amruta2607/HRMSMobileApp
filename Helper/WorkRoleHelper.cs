using MobileWebApi.Models.Responses;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Builds work-role lists and resolves the highest effective work role.
    /// </summary>
    public static class WorkRoleHelper
    {
        public const string DefaultWorkRoleName = "User";
        public const string AdminWorkRoleName = "Admin";
        public const string SuperAdminWorkRoleName = "SuperAdmin";

        /// <summary>
        /// Builds the work-role name list returned on login (default User + assigned roles, no duplicates).
        /// </summary>
        public static List<string> BuildLoginWorkRoles(IEnumerable<string>? assignedWorkRoleNames)
        {
            var roles = new List<string> { DefaultWorkRoleName };

            if (assignedWorkRoleNames == null)
                return roles;

            foreach (var name in assignedWorkRoleNames)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                var trimmed = name.Trim();
                if (roles.Any(r => r.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
                    continue;

                roles.Add(trimmed);
            }

            return roles;
        }

        /// <summary>
        /// Builds work-role items for the Me API (default User + assigned roles, no duplicates).
        /// </summary>
        public static List<WorkRoleDto> BuildMeWorkRoles(
            IEnumerable<WorkRoleDto>? assignedWorkRoles,
            int defaultUserRoleId)
        {
            var roles = new List<WorkRoleDto>
            {
                new() { Id = defaultUserRoleId, Name = DefaultWorkRoleName }
            };

            if (assignedWorkRoles == null)
                return roles;

            foreach (var role in assignedWorkRoles)
            {
                if (role == null || string.IsNullOrWhiteSpace(role.Name))
                    continue;

                var trimmed = role.Name.Trim();
                if (roles.Any(r => r.Name.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
                    continue;

                roles.Add(new WorkRoleDto
                {
                    Id = role.Id,
                    Name = trimmed
                });
            }

            return roles;
        }

        /// <summary>
        /// Resolves the highest work role using priority: SuperAdmin &gt; Admin &gt; User.
        /// </summary>
        public static string ResolvePrimaryWorkRole(IEnumerable<string>? workRoleNames)
        {
            if (workRoleNames == null)
                return DefaultWorkRoleName;

            var names = workRoleNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n.Trim())
                .ToList();

            if (names.Any(n => n.Equals(SuperAdminWorkRoleName, StringComparison.OrdinalIgnoreCase)))
                return SuperAdminWorkRoleName;

            if (names.Any(n => n.Equals(AdminWorkRoleName, StringComparison.OrdinalIgnoreCase)))
                return AdminWorkRoleName;

            return DefaultWorkRoleName;
        }

        /// <summary>
        /// Resolves dashboard data visibility scope from the primary work role.
        /// SuperAdmin sees all tenants; Admin and User see their own tenant.
        /// </summary>
        public static DashboardAccessScope ResolveDashboardAccessScope(string? primaryWorkRole)
        {
            if (!string.IsNullOrWhiteSpace(primaryWorkRole)
                && primaryWorkRole.Equals(SuperAdminWorkRoleName, StringComparison.OrdinalIgnoreCase))
            {
                return DashboardAccessScope.AllTenants;
            }

            // Admin and User share tenant-scoped visibility.
            return DashboardAccessScope.Tenant;
        }

        /// <summary>
        /// Returns true when the user's highest role is Admin or SuperAdmin.
        /// </summary>
        public static bool IsAdminOrSuperAdmin(IEnumerable<string>? workRoleNames)
        {
            var primary = ResolvePrimaryWorkRole(BuildLoginWorkRoles(workRoleNames));
            return primary.Equals(AdminWorkRoleName, StringComparison.OrdinalIgnoreCase)
                || primary.Equals(SuperAdminWorkRoleName, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Dashboard asset visibility scope derived from the user's highest work role.
    /// </summary>
    public enum DashboardAccessScope
    {
        /// <summary>SuperAdmin — all assets across every tenant.</summary>
        AllTenants = 0,

        /// <summary>Admin and User — all assets belonging to the user's tenant.</summary>
        Tenant = 1
    }
}
