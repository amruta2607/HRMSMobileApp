using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Ensures Location Tracking timestamp fields use
	/// yyyy-MM-ddTHH:mm:ss (no milliseconds, no timezone).
	/// </summary>
	public sealed class LocationTrackingTimestampSchemaFilter : ISchemaFilter
	{
		private const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss";
		private const string ExampleValue = "2026-08-20T10:00:00";

		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (schema == null)
			{
				return;
			}

			ApplyToSchema(schema);

			if (schema.Items != null)
			{
				ApplyToSchema(schema.Items);
			}
		}

		private static void ApplyToSchema(OpenApiSchema schema)
		{
			if (schema.Properties == null || schema.Properties.Count == 0)
			{
				return;
			}

			foreach (var (propertyName, propertySchema) in schema.Properties)
			{
				if (!propertyName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				propertySchema.Type = "string";
				propertySchema.Format = TimestampFormat;
				propertySchema.Example = new OpenApiString(ExampleValue);
				propertySchema.Description =
					$"Format: {TimestampFormat} (no milliseconds, no timezone). Example: {ExampleValue}";
			}
		}
	}
}
