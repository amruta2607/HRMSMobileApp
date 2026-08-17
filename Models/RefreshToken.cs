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

        /// <summary>
        /// Configured access-token lifetime in seconds (Jwt:AccessTokenExpiryInSeconds).
        /// </summary>
        public int AccessTokenExpiresIn { get; set; }

        /// <summary>
        /// Local/server access-token expiry in yyyy-MM-ddTHH:mm:ss (no Z, offset, or milliseconds).
        /// </summary>
        public string? AccessTokenExpiry { get; set; }

        public string? RefreshToken { get; set; }

        /// <summary>
        /// Configured refresh-token lifetime in hours (Jwt:RefreshTokenExpiryInHours).
        /// </summary>
        public int RefreshTokenExpiresIn { get; set; }

        /// <summary>
        /// Local/server refresh-token expiry in yyyy-MM-ddTHH:mm:ss (no Z, offset, or milliseconds).
        /// </summary>
        public string? RefreshTokenExpiry { get; set; }
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Same local/server access-token expiry as AccessTokenExpiry (yyyy-MM-ddTHH:mm:ss).
        /// </summary>
        public string? TokenExpiry { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }

        public int OrganisationId { get; set; }
        public bool AttendanceEnabled { get; set; }
		public bool IsGeoLocationEnabled { get; set; }
		public bool IsGeoFencingEnabled { get; set; }
		public bool IsActive { get; set; }

		public MobileAccessDto? ModuleAccess { get; set; }
		public decimal? Latitude { get; set; }

		public decimal? Longitude { get; set; }

		public int? Radius { get; set; }

		public string? LocationAddress { get; set; }

		/// <summary>
		/// Active work role names for the logged-in user. Always includes the default "User" role.
		/// </summary>
		public List<string> WorkRoles { get; set; } = new() { "User" };
	}
}

