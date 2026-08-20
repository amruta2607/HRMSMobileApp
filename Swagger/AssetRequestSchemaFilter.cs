using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MobileWebApi.Models.Requests;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
    /// <summary>
    /// Sets integer examples to 0 for asset and handover request schemas in Swagger UI.
    /// </summary>
    public sealed class AssetRequestSchemaFilter : ISchemaFilter
    {
        private static readonly HashSet<Type> SupportedTypes = new()
        {
            typeof(CreateAssetRequest),
            typeof(UpdateAssetRequest),
            typeof(AssetHandoverRequest),
            typeof(UpdateAssetHandoverRequest),
            typeof(CreateAssetMaintenanceRequest)
        };

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null || schema.Properties.Count == 0)
                return;

            if (!SupportedTypes.Contains(context.Type))
                return;

            foreach (var propertySchema in schema.Properties.Values)
            {
                SetIntegerExampleToZero(propertySchema);
            }
        }

        private static void SetIntegerExampleToZero(OpenApiSchema propertySchema)
        {
            if (propertySchema.Type == "integer")
            {
                propertySchema.Example = new OpenApiInteger(0);
                return;
            }

            if (propertySchema.Type == "number" && propertySchema.Format == "double")
            {
                propertySchema.Example = new OpenApiDouble(0);
            }
        }
    }
}
