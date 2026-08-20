using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Sets Location Tracking request-body examples to use
	/// timestamp format yyyy-MM-ddTHH:mm:ss (no ms / timezone).
	/// </summary>
	public sealed class LocationTrackingTimestampOperationFilter : IOperationFilter
	{
		private const string ExampleTimestamp = "2026-08-20T10:00:00";

		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			if (!IsLocationTrackingOperation(context))
			{
				return;
			}

			if (operation.RequestBody?.Content == null)
			{
				return;
			}

			foreach (var mediaType in operation.RequestBody.Content.Values)
			{
				ApplySchemaTimestampExamples(mediaType.Schema);

				if (mediaType.Schema?.Properties == null)
				{
					continue;
				}

				// Full Try-it-out body for batch add.
				if (mediaType.Schema.Properties.ContainsKey("locations") ||
				    mediaType.Schema.Properties.ContainsKey("Locations"))
				{
					mediaType.Example = new OpenApiObject
					{
						["user_id"] = new OpenApiInteger(0),
						["locations"] = new OpenApiArray
						{
							new OpenApiObject
							{
								["latitude"] = new OpenApiDouble(0),
								["longitude"] = new OpenApiDouble(0),
								["timestamp"] = new OpenApiString(ExampleTimestamp),
								["location_from"] = new OpenApiString("string")
							}
						}
					};
				}
				else if (mediaType.Schema.Properties.ContainsKey("timestamp") ||
				         mediaType.Schema.Properties.ContainsKey("Timestamp"))
				{
					mediaType.Example = BuildSingleLocationExample(mediaType.Schema);
				}
			}
		}

		private static bool IsLocationTrackingOperation(OperationFilterContext context)
		{
			if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor action)
			{
				return false;
			}

			var controller = action.ControllerName ?? string.Empty;
			var route = context.ApiDescription.RelativePath ?? string.Empty;

			return controller.Contains("LocationTracking", StringComparison.OrdinalIgnoreCase) ||
			       route.Contains("location-tracking", StringComparison.OrdinalIgnoreCase);
		}

		private static void ApplySchemaTimestampExamples(OpenApiSchema? schema)
		{
			if (schema?.Properties == null)
			{
				return;
			}

			foreach (var (name, propertySchema) in schema.Properties)
			{
				if (name.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
				{
					propertySchema.Type = "string";
					propertySchema.Format = "yyyy-MM-ddTHH:mm:ss";
					propertySchema.Example = new OpenApiString(ExampleTimestamp);
				}

				if (propertySchema.Items != null)
				{
					ApplySchemaTimestampExamples(propertySchema.Items);
				}

				if (propertySchema.Properties != null)
				{
					ApplySchemaTimestampExamples(propertySchema);
				}
			}
		}

		private static OpenApiObject BuildSingleLocationExample(OpenApiSchema schema)
		{
			var example = new OpenApiObject();

			foreach (var (name, _) in schema.Properties)
			{
				if (name.Equals("user_id", StringComparison.OrdinalIgnoreCase) ||
				    name.Equals("employee_id", StringComparison.OrdinalIgnoreCase))
				{
					example[name] = new OpenApiInteger(0);
				}
				else if (name.Equals("latitude", StringComparison.OrdinalIgnoreCase) ||
				         name.Equals("longitude", StringComparison.OrdinalIgnoreCase))
				{
					example[name] = new OpenApiDouble(0);
				}
				else if (name.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
				{
					example[name] = new OpenApiString(ExampleTimestamp);
				}
				else if (name.Equals("location_from", StringComparison.OrdinalIgnoreCase) ||
				         name.Equals("issue_type", StringComparison.OrdinalIgnoreCase) ||
				         name.Equals("description", StringComparison.OrdinalIgnoreCase))
				{
					example[name] = new OpenApiString("string");
				}
			}

			return example;
		}
	}
}
