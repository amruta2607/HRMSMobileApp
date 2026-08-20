namespace MobileWebApi.Helper
{
    /// <summary>
    /// Normalizes optional request values so empty / default values become SQL NULL.
    /// </summary>
    public static class OptionalValueHelper
    {
        /// <summary>
        /// Converts null, whitespace, and the literal text "NULL" to null; otherwise returns trimmed text.
        /// </summary>
        public static string? NullIfEmpty(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();
            if (trimmed.Equals("NULL", StringComparison.OrdinalIgnoreCase))
                return null;

            return trimmed;
        }

        /// <summary>
        /// Converts missing or non-positive optional foreign keys to null.
        /// </summary>
        public static int? NullIfNonPositive(int? value)
            => value.HasValue && value.Value > 0 ? value.Value : null;

        /// <summary>
        /// Converts non-positive optional foreign keys to null.
        /// </summary>
        public static int? NullIfNonPositive(int value)
            => value > 0 ? value : null;

        /// <summary>
        /// Converts missing or non-positive optional decimals to null.
        /// </summary>
        public static decimal? NullIfNonPositive(decimal? value)
            => value.HasValue && value.Value > 0 ? value.Value : null;

        /// <summary>
        /// Converts missing or non-positive optional doubles to null.
        /// </summary>
        public static double? NullIfNonPositive(double? value)
            => value.HasValue && value.Value > 0 ? value.Value : null;

        /// <summary>
        /// Converts missing or <see cref="DateTime.MinValue"/> dates to null.
        /// </summary>
        public static DateTime? NullIfDefault(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            if (value.Value == DateTime.MinValue || value.Value.Year <= 1)
                return null;

            return value.Value;
        }

        /// <summary>
        /// Converts <see cref="DateTime.MinValue"/> dates to null.
        /// </summary>
        public static DateTime? NullIfDefault(DateTime value)
            => NullIfDefault((DateTime?)value);
    }
}
