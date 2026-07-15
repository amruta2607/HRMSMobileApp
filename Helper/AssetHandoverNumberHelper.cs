using Dapper;
using MobileWebApi.Resources;
using System.Data;
using System.Globalization;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Generates tenant-scoped asset handover numbers.
    /// </summary>
    public static class AssetHandoverNumberHelper
    {
        /// <summary>
        /// Generates the next handover number in the format AHO/yyyyMMdd0001.
        /// </summary>
        public static string GenerateNextNumber(
            IDbConnection connection,
            int tenantId,
            QueryProvider queries,
            IDbTransaction transaction)
        {
            var prefix = "AHO/" + DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
            var prefixPattern = prefix + "%";

            var maxNumber = connection.QueryFirstOrDefault<string>(
                queries.Get("AssetHandOver_GetMaxNumber"),
                new { TenantId = tenantId, PrefixPattern = prefixPattern },
                transaction);

            long nextNumber = 1;
            if (!string.IsNullOrEmpty(maxNumber) &&
                maxNumber.Length > prefix.Length &&
                long.TryParse(maxNumber[prefix.Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var current))
            {
                nextNumber = current + 1;
            }

            return prefix + nextNumber.ToString("D4", CultureInfo.InvariantCulture);
        }
    }
}
