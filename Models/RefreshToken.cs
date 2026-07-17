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
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }

        public int OrganisationId { get; set; }
        public bool AttendanceEnabled { get; set; }
        public bool EnableLocationTracking { get; set; }
        public bool EnableEmployeeLevelLocationTracking { get; set; }
        public bool EmployeeLocationTrackingEnabled { get; set; }
		public bool IsGeoLocationEnabled { get; set; }
		public bool IsGeoFencingEnabled { get; set; }
		public bool IsActive { get; set; }

		public MobileAccessDto? ModuleAccess { get; set; }
		public decimal? Latitude { get; set; }

		public decimal? Longitude { get; set; }

		public int? Radius { get; set; }

		public string? LocationAddress { get; set; }

		/// <summary>
		/// Active work roles for the logged-in user. Always includes the default "User" role.
		/// </summary>
		public List<WorkRole> WorkRoles { get; set; } = new()
		{
			new WorkRole { WorkRoleId = 0, WorkRoleName = "User" }
		};
	}
}

