using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Sets OpenAPI examples on Attendance DateTime request properties
	/// (e.g. attendance_date, punch_in_time, punch_out_time) while keeping
	/// schema type string and format date-time.
	/// </summary>
	/// <remarks>
	/// Swagger UI limitation: input placeholders may still show the property name;
	/// the Example value (e.g. 2026-07-22T09:15:35) is shown in the schema docs.
	/// </remarks>
	public sealed class AttendanceDateTimeSchemaFilter : ISchemaFilter
	{
		private static readonly HashSet<string> KnownDateTimeFields = new(StringComparer.OrdinalIgnoreCase)
		{
			"attendance_date",
			"punch_in_time",
			"punch_out_time",
			"punchTime",
			"from_date",
			"to_date",
			"fromDate",
			"toDate",
			"DateFrom",
			"DateTo",
			"CalendarDate",
			"FromDate",
			"ToDate",
			"date",
			"disputeDate",
			"DisputeDate",
			"requestedPunchInTime",
			"RequestedPunchInTime",
			"requestedPunchOutTime",
			"RequestedPunchOutTime"
		};

		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (schema?.Properties == null || schema.Properties.Count == 0)
				return;

			foreach (var (propertyName, propertySchema) in schema.Properties)
			{
				var isKnownField = KnownDateTimeFields.Contains(propertyName);
				var isDateTimeFormat = string.Equals(propertySchema.Format, "date-time", StringComparison.OrdinalIgnoreCase);

				if (!isKnownField && !isDateTimeFormat)
					continue;

				// For generic date-time properties, only touch Attendance/Punch request schemas.
				if (!isKnownField && !IsAttendanceRequestType(context.Type))
					continue;

				propertySchema.Type = "string";
				propertySchema.Format = "date-time";
				propertySchema.Example = AttendanceDateTimeSwaggerExamples.ToOpenApiString(propertyName);

				if (string.IsNullOrWhiteSpace(propertySchema.Description))
				{
					propertySchema.Description =
						$"Format: {AttendanceDateTimeSwaggerExamples.FormatHint}. Example: {AttendanceDateTimeSwaggerExamples.Resolve(propertyName)}";
				}
			}
		}

		private static bool IsAttendanceRequestType(Type? type)
		{
			if (type == null)
				return false;

			var name = type.Name;
			return name.Contains("Punch", StringComparison.OrdinalIgnoreCase) ||
			       name.Contains("Attendance", StringComparison.OrdinalIgnoreCase) ||
			       name.Contains("Dispute", StringComparison.OrdinalIgnoreCase);
		}
	}
}
