namespace MobileWebApi.Models.Responses
{
    /// <summary>
    /// Work role identifier and display name.
    /// </summary>
    public class WorkRoleDto
    {
        /// <summary>
        /// Work role identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Work role name.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
