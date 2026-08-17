namespace MobileWebApi.Models
{
	public class TokenResponse
	{
		public string Token { get; set; } = string.Empty;
        public string Username { get; internal set; }

		public int UserId { get; set; }
    }
}
