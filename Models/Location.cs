namespace MobileWebApi.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusMeters { get; set; }

        public int OrganizationId { get; set; }
        public int BranchId { get; set; }

        public Organization? Organization { get; set; }
        public Branch? Branch { get; set; }
    }
}
