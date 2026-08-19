namespace MobileWebApi.Models.Responses
{
	/// <summary>
	/// Response payload for retrieving an asset QR code.
	/// </summary>
	public class AssetQrCodeResponse
	{
		public bool Success { get; set; } = true;

		public int AssetId { get; set; }

		public string? AssetTagNumber { get; set; }

		public string? AssetName { get; set; }

		/// <summary>
		/// QR code path as stored on the Asset record (returned as-is).
		/// </summary>
		public string QrCode { get; set; } = string.Empty;
	}
}