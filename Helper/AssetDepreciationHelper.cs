using Dapper;
using MobileWebApi.Resources;
using System.Data;

namespace MobileWebApi.Helper
{
    /// <summary>
    /// Asset depreciation calculations aligned with the Serenity Asset module.
    /// </summary>
    public static class AssetDepreciationHelper
    {
        /// <summary>
        /// Straight-line monthly depreciation.
        /// </summary>
        public static double CalculateActualValue(
            double purchasePrice,
            double yearlyDepreciationPercentage,
            DateTime? purchaseDate,
            DateTime? asOf = null)
        {
            if (purchasePrice <= 0 || yearlyDepreciationPercentage <= 0 || purchaseDate == null)
                return purchasePrice;

            var now = (asOf ?? DateTime.Today).Date;
            var start = purchaseDate.Value.Date;

            if (now <= start)
                return purchasePrice;

            var months = ((now.Year - start.Year) * 12) + (now.Month - start.Month);
            if (now.Day < start.Day)
                months--;

            if (months <= 0)
                return purchasePrice;

            var monthlyRate = (yearlyDepreciationPercentage / 12.0) / 100.0;
            var monthlyDepreciation = purchasePrice * monthlyRate;
            var totalDepreciation = monthlyDepreciation * months;
            var actual = purchasePrice - totalDepreciation;

            return actual < 0 ? 0 : Math.Round(actual, 2);
        }

        /// <summary>
        /// Reads the yearly depreciation percentage configured on the asset category.
        /// </summary>
        public static double GetCategoryYearlyPercentage(
            IDbConnection connection,
            int categoryId,
            int tenantId,
            QueryProvider queries,
            IDbTransaction? transaction = null)
        {
            return connection.QueryFirstOrDefault<double?>(
                queries.Get("Asset_GetCategoryDepreciationPercentage"),
                new { CategoryId = categoryId, TenantId = tenantId },
                transaction) ?? 0;
        }
    }
}
