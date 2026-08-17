namespace MobileWebApi.Models
{
    public class TenantConfiguration
    {
        public string? EmployeeNoPrefix { get; set; }
        public int? EmployeeNoStartWith { get; set; }
		public bool IsGeoLocationEnabled { get; set; }
		public bool IsGeoFencingEnabled { get; set; }
		public bool IsActive { get; set; }
        public decimal Latitude { get; internal set; }
        public decimal Longitude { get; internal set; }
        public int Radius { get; internal set; }
        public string? LocationAddress { get; internal set; }
    }

}
