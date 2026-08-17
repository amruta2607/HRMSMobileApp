using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace MobileWebApi.Helper
{
	/// <summary>
	/// Parses scanned QR values and builds absolute asset media URLs.
	/// </summary>
	public static class AssetQrScannerHelper
	{
		private static readonly Regex ViewByQrRegex = new(
			@"ViewByQR/(\d+)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static readonly Regex EditAssetRegex = new(
			@"Asset#edit/(\d+)",
			RegexOptions.IgnoreCase | RegexOptions.Compiled);

		/// <summary>
		/// Attempts to extract an asset identifier from a scanned QR value.
		/// Supports:
		/// 8
		/// https://localhost:44304/Asset/ViewByQR/8
		/// https:%2F%2Flocalhost:44304%2FAsset%2FViewByQR%2F8
		/// </summary>
		public static bool TryParseAssetId(string scannedText, out int assetId)
		{
			assetId = 0;

			if (string.IsNullOrWhiteSpace(scannedText))
				return false;

			// Decode URL-encoded QR values
			var text = WebUtility.UrlDecode(scannedText).Trim();

			// Raw Id
			if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out assetId))
				return true;

			// Match ViewByQR/{id}
			var viewMatch = ViewByQrRegex.Match(text);
			if (viewMatch.Success &&
				int.TryParse(viewMatch.Groups[1].Value,
					NumberStyles.None,
					CultureInfo.InvariantCulture,
					out assetId))
			{
				return true;
			}

			// Match Asset#edit/{id}
			var editMatch = EditAssetRegex.Match(text);
			if (editMatch.Success &&
				int.TryParse(editMatch.Groups[1].Value,
					NumberStyles.None,
					CultureInfo.InvariantCulture,
					out assetId))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns true if the scanned value appears to be a QR payload.
		/// </summary>
		public static bool LooksLikeQrPayload(string scannedText)
		{
			if (string.IsNullOrWhiteSpace(scannedText))
				return false;

			var text = WebUtility.UrlDecode(scannedText).Trim();

			return text.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
				|| text.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
				|| ViewByQrRegex.IsMatch(text)
				|| EditAssetRegex.IsMatch(text);
		}

		/// <summary>
		/// Converts a relative upload path to an absolute URL.
		/// </summary>
		public static string ToAbsoluteUrl(string? relativePath, string baseUrl)
		{
			if (string.IsNullOrWhiteSpace(relativePath))
				return string.Empty;

			if (relativePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
				relativePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				return relativePath;
			}

			if (string.IsNullOrWhiteSpace(baseUrl))
				return relativePath;

			return $"{baseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
		}
	}
}