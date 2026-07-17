using MobileWebApi.Models;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Builds the work-role list returned on login (default User + assigned roles, no duplicates).
    /// </summary>
    public static class WorkRoleHelper
    {
        public const string DefaultWorkRoleName = "User";

        public static List<WorkRole> BuildLoginWorkRoles(
            IEnumerable<WorkRole>? assignedWorkRoles,
            WorkRole? defaultUserWorkRole = null)
        {
            var roles = new List<WorkRole>();

            var defaultRole = defaultUserWorkRole != null
                && !string.IsNullOrWhiteSpace(defaultUserWorkRole.WorkRoleName)
                    ? new WorkRole
                    {
                        WorkRoleId = defaultUserWorkRole.WorkRoleId,
                        WorkRoleName = defaultUserWorkRole.WorkRoleName.Trim()
                    }
                    : new WorkRole
                    {
                        WorkRoleId = 0,
                        WorkRoleName = DefaultWorkRoleName
                    };

            if (!defaultRole.WorkRoleName.Equals(DefaultWorkRoleName, StringComparison.OrdinalIgnoreCase))
            {
                defaultRole = new WorkRole
                {
                    WorkRoleId = defaultRole.WorkRoleId,
                    WorkRoleName = DefaultWorkRoleName
                };
            }

            roles.Add(defaultRole);

            if (assignedWorkRoles == null)
                return roles;

            foreach (var role in assignedWorkRoles)
            {
                if (role == null || string.IsNullOrWhiteSpace(role.WorkRoleName))
                    continue;

                var trimmedName = role.WorkRoleName.Trim();
                var existing = roles.FirstOrDefault(r =>
                    r.WorkRoleId == role.WorkRoleId
                    || r.WorkRoleName.Equals(trimmedName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    // Prefer a real WorkRoleId when the default User was added with Id 0.
                    if (existing.WorkRoleId <= 0 && role.WorkRoleId > 0)
                        existing.WorkRoleId = role.WorkRoleId;
                    continue;
                }

                roles.Add(new WorkRole
                {
                    WorkRoleId = role.WorkRoleId,
                    WorkRoleName = trimmedName
                });
            }

            return roles;
        }
    }
}
