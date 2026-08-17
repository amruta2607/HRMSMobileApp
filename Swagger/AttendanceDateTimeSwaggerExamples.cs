using Microsoft.OpenApi.Any;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Shared OpenAPI example values for Attendance DateTime request fields.
	/// Format: yyyy-MM-ddTHH:mm:ss (no milliseconds / timezone offset).
	/// </summary>
	/// <remarks>
	/// Swagger UI limitation: some UI versions keep the property name as the
	/// input placeholder and only surface these values under the field Example /
	/// schema documentation. OpenAPI still emits string($date-time) with Example set.
	/// </remarks>
	internal static class AttendanceDateTimeSwaggerExamples
	{
		public const string FormatHint = "yyyy-MM-ddTHH:mm:ss";

		public const string AttendanceDate = "2026-07-22T00:00:00";
		public const string PunchInTime = "2026-07-22T09:15:35";
		public const string PunchOutTime = "2026-07-22T18:00:00";
		public const string GenericDateTime = "2026-07-22T09:15:35";
		public const string GenericDate = "2026-07-22T00:00:00";

		public static string Resolve(string? propertyName)
		{
			if (string.IsNullOrWhiteSpace(propertyName))
				return GenericDateTime;

			if (propertyName.Equals("attendance_date", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("CalendarDate", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("date", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("from_date", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("fromDate", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("DateFrom", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("FromDate", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("to_date", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("toDate", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("DateTo", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("ToDate", StringComparison.OrdinalIgnoreCase))
			{
				return propertyName.Equals("attendance_date", StringComparison.OrdinalIgnoreCase)
					? AttendanceDate
					: GenericDate;
			}

			if (propertyName.Equals("punch_in_time", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("punchTime", StringComparison.OrdinalIgnoreCase))
			{
				return PunchInTime;
			}

			if (propertyName.Equals("punch_out_time", StringComparison.OrdinalIgnoreCase))
			{
				return PunchOutTime;
			}

			if (propertyName.Equals("disputeDate", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("DisputeDate", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("requestedPunchOutTime", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("RequestedPunchOutTime", StringComparison.OrdinalIgnoreCase))
			{
				return PunchOutTime;
			}

			if (propertyName.Equals("requestedPunchInTime", StringComparison.OrdinalIgnoreCase) ||
			    propertyName.Equals("RequestedPunchInTime", StringComparison.OrdinalIgnoreCase))
			{
				return "2026-07-22T09:00:00";
			}

			return GenericDateTime;
		}

		public static OpenApiString ToOpenApiString(string? propertyName)
			=> new(Resolve(propertyName));
	}
}
