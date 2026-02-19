namespace MobileWebApi.Models
{
    public class TenantConfiguration
    {
        public string? EmployeeNoPrefix { get; set; }
        public int? EmployeeNoStartWith { get; set; }
		public bool IsGeoLocationEnabled { get; set; }
		public bool IsGeoFencingEnabled { get; set; }
	}

}
