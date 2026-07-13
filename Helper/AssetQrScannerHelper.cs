using System.Globalization;
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
        /// Attempts to extract an asset identifier from a scanned QR value or raw id.
        /// </summary>
        public static bool TryParseAssetId(string scannedText, out int assetId)
        {
            assetId = 0;
            if (string.IsNullOrWhiteSpace(scannedText))
                return false;

            var text = scannedText.Trim();

            if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out assetId))
                return true;

            var viewMatch = ViewByQrRegex.Match(text);
            if (viewMatch.Success &&
                int.TryParse(viewMatch.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out assetId))
            {
                return true;
            }

            var editMatch = EditAssetRegex.Match(text);
            if (editMatch.Success &&
                int.TryParse(editMatch.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out assetId))
            {
                return true;
            }

            return false;
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
