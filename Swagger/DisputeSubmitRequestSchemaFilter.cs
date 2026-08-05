using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MobileWebApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Documents DisputeSubmitRequest DateTime fields as yyyy-MM-ddTHH:mm:ss
	/// and provides a complete Swagger example request.
	/// </summary>
	public sealed class DisputeSubmitRequestSchemaFilter : ISchemaFilter
	{
		private const string FormatHint = "yyyy-MM-ddTHH:mm:ss";
		private const string DisputeDateExample = "2026-07-22T18:00:00";
		private const string RequestedPunchInExample = "2026-07-22T09:00:00";
		private const string RequestedPunchOutExample = "2026-07-22T18:00:00";

		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (context.Type != typeof(DisputeSubmitRequest) || schema?.Properties == null)
				return;

			ApplyDateTimeProperty(schema, "disputeDate", DisputeDateExample,
				"Dispute date/time. Format: yyyy-MM-ddTHH:mm:ss. Example: 2026-07-22T18:00:00");
			ApplyDateTimeProperty(schema, "requestedPunchInTime", RequestedPunchInExample,
				"Requested punch-in time. Format: yyyy-MM-ddTHH:mm:ss. Example: 2026-07-22T09:00:00");
			ApplyDateTimeProperty(schema, "requestedPunchOutTime", RequestedPunchOutExample,
				"Requested punch-out time. Format: yyyy-MM-ddTHH:mm:ss. Example: 2026-07-22T18:00:00");

			// Also match PascalCase keys if naming policy is not camelCase.
			ApplyDateTimeProperty(schema, "DisputeDate", DisputeDateExample,
				"Dispute date/time. Format: yyyy-MM-ddTHH:mm:ss. Example: 2026-07-22T18:00:00");
			ApplyDateTimeProperty(schema, "RequestedPunchInTime", RequestedPunchInExample,
				"Requested punch-in time. Format: yyyy-MM-ddTHH:mm:ss. Example: 2026-07-22T09:00:00");
			ApplyDateTimeProperty(schema, "RequestedPunchOutTime", RequestedPunchOutExample,
				"Requested punch-out time. Format: yyyy-MM-ddTHH:mm:ss. Example: 2026-07-22T18:00:00");

			schema.Example = new OpenApiObject
			{
				["userId"] = new OpenApiInteger(1),
				["employeeId"] = new OpenApiInteger(10),
				["disputeCategoryId"] = new OpenApiInteger(2),
				["disputeDate"] = new OpenApiString(DisputeDateExample),
				["description"] = new OpenApiString("Forgot to punch in."),
				["punchId"] = new OpenApiInteger(125),
				["requestedPunchInTime"] = new OpenApiString(RequestedPunchInExample),
				["requestedPunchOutTime"] = new OpenApiString(RequestedPunchOutExample)
			};

			schema.Description =
				$"DateTime fields use format {FormatHint} (no milliseconds / timezone offset).";
		}

		private static void ApplyDateTimeProperty(
			OpenApiSchema schema,
			string propertyName,
			string example,
			string description)
		{
			if (!schema.Properties.TryGetValue(propertyName, out var propertySchema))
				return;

			propertySchema.Type = "string";
			propertySchema.Format = "date-time";
			propertySchema.Example = new OpenApiString(example);
			propertySchema.Description = description;
		}
	}
}
