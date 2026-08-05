using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Applies Attendance DateTime examples to query/path/header parameters and
	/// multipart form-data request bodies used by Attendance APIs.
	/// </summary>
	/// <remarks>
	/// Swagger UI limitation: form/query input placeholders may still display the
	/// property name; OpenAPI Example values document the expected
	/// yyyy-MM-ddTHH:mm:ss format (e.g. 2026-07-22T09:15:35).
	/// </remarks>
	public sealed class AttendanceDateTimeOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if (!IsAttendanceOperation(context))
				return;

			ApplyParameterExamples(operation);
			ApplyRequestBodyExamples(operation);
		}

		private static bool IsAttendanceOperation(OperationFilterContext context)
		{
			if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor action)
				return false;

			var controller = action.ControllerName ?? string.Empty;
			var route = context.ApiDescription.RelativePath ?? string.Empty;

			return controller.Contains("Attendance", StringComparison.OrdinalIgnoreCase) ||
			       controller.Contains("Dispute", StringComparison.OrdinalIgnoreCase) ||
			       route.Contains("attendance", StringComparison.OrdinalIgnoreCase) ||
			       route.Contains("punch", StringComparison.OrdinalIgnoreCase) ||
			       route.Contains("dispute", StringComparison.OrdinalIgnoreCase);
		}

		private static void ApplyParameterExamples(OpenApiOperation operation)
		{
			if (operation.Parameters == null)
				return;

			foreach (var parameter in operation.Parameters)
			{
				if (parameter.Schema == null)
					continue;

				if (!string.Equals(parameter.Schema.Format, "date-time", StringComparison.OrdinalIgnoreCase) &&
				    !IsKnownDateTimeName(parameter.Name))
				{
					continue;
				}

				parameter.Schema.Type = "string";
				parameter.Schema.Format = "date-time";
				parameter.Schema.Example = AttendanceDateTimeSwaggerExamples.ToOpenApiString(parameter.Name);
				parameter.Example = AttendanceDateTimeSwaggerExamples.ToOpenApiString(parameter.Name);

				if (string.IsNullOrWhiteSpace(parameter.Description))
				{
					parameter.Description =
						$"Format: {AttendanceDateTimeSwaggerExamples.FormatHint}. Example: {AttendanceDateTimeSwaggerExamples.Resolve(parameter.Name)}";
				}
			}
		}

		private static void ApplyRequestBodyExamples(OpenApiOperation operation)
		{
			if (operation.RequestBody?.Content == null)
				return;

			foreach (var (contentType, mediaType) in operation.RequestBody.Content)
			{
				ApplySchemaPropertyExamples(mediaType.Schema);

				// Do NOT set media-type level Example for multipart/form-data.
				// A partial Example object (e.g. only DateTime fields) causes Swagger UI to
				// omit IFormFile / format:binary fields, so the Choose File control disappears.
				if (contentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
					continue;

				if (mediaType.Schema?.Properties == null || mediaType.Schema.Properties.Count == 0)
					continue;

				if (HasBinaryProperty(mediaType.Schema))
					continue;

				// Keep complete schema-level examples (e.g. DisputeSubmitRequest) intact.
				if (mediaType.Schema.Example != null)
					continue;

				var exampleObject = new Microsoft.OpenApi.Any.OpenApiObject();
				foreach (var (name, propertySchema) in mediaType.Schema.Properties)
				{
					if (!string.Equals(propertySchema.Format, "date-time", StringComparison.OrdinalIgnoreCase) &&
					    !IsKnownDateTimeName(name))
					{
						continue;
					}

					exampleObject[name] = AttendanceDateTimeSwaggerExamples.ToOpenApiString(name);
				}

				if (exampleObject.Count > 0)
					mediaType.Example = exampleObject;
			}
		}

		private static bool HasBinaryProperty(OpenApiSchema schema)
		{
			return schema.Properties != null &&
			       schema.Properties.Values.Any(p =>
				       string.Equals(p.Format, "binary", StringComparison.OrdinalIgnoreCase));
		}

		private static void ApplySchemaPropertyExamples(OpenApiSchema? schema)
		{
			if (schema?.Properties == null)
				return;

			foreach (var (name, propertySchema) in schema.Properties)
			{
				if (!string.Equals(propertySchema.Format, "date-time", StringComparison.OrdinalIgnoreCase) &&
				    !IsKnownDateTimeName(name))
				{
					continue;
				}

				propertySchema.Type = "string";
				propertySchema.Format = "date-time";
				propertySchema.Example = AttendanceDateTimeSwaggerExamples.ToOpenApiString(name);
			}
		}

		private static bool IsKnownDateTimeName(string? name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return false;

			return name.Equals("attendance_date", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("punch_in_time", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("punch_out_time", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("punchTime", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("from_date", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("to_date", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("fromDate", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("toDate", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("DateFrom", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("DateTo", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("CalendarDate", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("FromDate", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("ToDate", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("date", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("disputeDate", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("DisputeDate", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("requestedPunchInTime", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("RequestedPunchInTime", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("requestedPunchOutTime", StringComparison.OrdinalIgnoreCase) ||
			       name.Equals("RequestedPunchOutTime", StringComparison.OrdinalIgnoreCase);
		}
	}
}
