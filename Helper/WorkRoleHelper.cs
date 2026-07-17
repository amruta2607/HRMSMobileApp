namespace MobileWebApi.Helper
{
    /// <summary>
    /// Builds the work-role name list returned on login (default User + assigned roles, no duplicates).
    /// </summary>
    public static class WorkRoleHelper
    {
        public const string DefaultWorkRoleName = "User";

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
    }
}
