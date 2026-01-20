namespace MobileWebApi.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string BranchName { get; set; } = string.Empty;

        public int OrganizationId { get; set; }
        public Organization? Organization { get; set; }
    }
}
