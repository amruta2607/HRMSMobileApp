namespace MobileWebApi.Helper
{
    /// <summary>
    /// Category names must match DisputeCategory.CategoryName values in the database.
    /// Mirrors Web HRMS AttendanceDisputeCategories.
    /// </summary>
    public static class AttendanceDisputeCategories
    {
        public const string MissingCheckOut = "Missing Check-Out";
        public const string WrongCheckInTime = "Wrong Check-In Time";
        public const string WrongCheckOutTime = "Wrong Check-Out Time";
        public const string AttendanceNotMarked = "Attendance Not Marked";
        public const string Other = "Other";

        public static bool EqualsName(string? categoryName, string expected) =>
            string.Equals((categoryName ?? string.Empty).Trim(), expected, StringComparison.OrdinalIgnoreCase);

        public static bool UpdatesPunchIn(string? categoryName) =>
            EqualsName(categoryName, WrongCheckInTime) || EqualsName(categoryName, AttendanceNotMarked);

        public static bool UpdatesPunchOut(string? categoryName) =>
            EqualsName(categoryName, MissingCheckOut) ||
            EqualsName(categoryName, WrongCheckOutTime) ||
            EqualsName(categoryName, AttendanceNotMarked);

        public static bool AppliesPunchCorrection(string? categoryName) =>
            !EqualsName(categoryName, Other);
    }
}
