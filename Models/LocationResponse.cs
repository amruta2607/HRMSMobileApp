namespace MobileWebApi.Models
{
    public class LocationResponse
    {
        public int Id { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int RadiusMeters { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string Company_Name { get; set; } = string.Empty;
    }
}
