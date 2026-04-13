namespace MobileWebApi.Models
{
    /// <summary>
    /// Refresh token model
    /// </summary>
 
    /// <summary>
    /// Request model for refresh token
    /// </summary>
   
    /// <summary>
    /// Response model for token
    /// </summary>
    public class TokenWithRefreshResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }

        public int OrganisationId { get; set; }
		public bool IsGeoLocationEnabled { get; set; }
		public bool IsGeoFencingEnabled { get; set; }

        public MobileAccessDto? ModuleAccess { get; set; }
	}
}

