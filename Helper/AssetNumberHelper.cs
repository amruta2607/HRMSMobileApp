using Dapper;
using MobileWebApi.Resources;
using MobileWebApi.Services;
using System.Data;
using System.Globalization;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Generates tenant-scoped asset numbers using the same logic as the web application.
    /// </summary>
    public static class AssetNumberHelper
    {
        private sealed class TenantNumberConfig
        {
            public string? AssetNumberPrefix { get; set; }
            public bool? AssetNumberUseDate { get; set; }
            public short? AssetNumberLength { get; set; }
        }

        /// <summary>
        /// Generates the next asset number for the specified tenant.
        /// </summary>
        public static string GenerateNextNumber(
            IDbConnection connection,
            int tenantId,
            QueryProvider queries,
            IDbTransaction? transaction = null)
        {
            var tenant = connection.QueryFirstOrDefault<TenantNumberConfig>(
                queries.Get("Asset_GetTenantNumberConfig"),
                new { TenantId = tenantId },
                transaction);

            if (tenant == null)
                throw new AssetValidationException(Constants.AssetMessages.TenantConfigurationNotFound);

            var prefix = tenant.AssetNumberUseDate == true
                ? (tenant.AssetNumberPrefix ?? "AST") + "/" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                : tenant.AssetNumberPrefix ?? "AST";

            var length = (int)(tenant.AssetNumberLength ?? 16);
            var minValue = prefix.PadRight(length, '0');
            var maxValue = prefix.PadRight(length, '9');
            var prefixPattern = prefix + "%";

            var maxNumber = connection.QueryFirstOrDefault<string>(
                queries.Get("Asset_GetMaxNumber"),
                new
                {
                    TenantId = tenantId,
                    PrefixPattern = prefixPattern,
                    MinValue = minValue,
                    MaxValue = maxValue
                },
                transaction);

            long nextNumber = 1;
            if (!string.IsNullOrEmpty(maxNumber) &&
                maxNumber.Length > prefix.Length &&
                long.TryParse(maxNumber[prefix.Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var current))
            {
                nextNumber = current + 1;
            }

            var effectiveLength = length;
            if (effectiveLength <= prefix.Length)
            {
                effectiveLength = prefix.Length + nextNumber.ToString(CultureInfo.InvariantCulture).Length + 3;
            }

            return prefix + nextNumber.ToString(CultureInfo.InvariantCulture)
                .PadLeft(effectiveLength - prefix.Length, '0');
        }
    }
}
