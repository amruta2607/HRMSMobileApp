using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Ensures Location Tracking timestamp examples show
	/// yyyy-MM-ddTHH:mm:ss (without milliseconds or timezone).
	/// </summary>
	public sealed class LocationTrackingTimestampSchemaFilter : ISchemaFilter
	{
		private const string TimestampFormat = "yyyy-MM-ddTHH:mm:ss";

		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (schema?.Properties == null || schema.Properties.Count == 0)
			{
				return;
			}

			foreach (var (propertyName, propertySchema) in schema.Properties)
			{
				if (!propertyName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (propertySchema.Type == "string" &&
					propertySchema.Format == "date-time")
				{
					propertySchema.Example = new OpenApiString(
						DateTime.UtcNow.ToString(TimestampFormat)
					);
				}
			}
		}
	}
}