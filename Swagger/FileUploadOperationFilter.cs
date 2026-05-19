using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
    /// <summary>
    /// Builds multipart/form-data request bodies from individual [FromForm] parameters
    /// so Swagger UI shows separate fields (not an empty JSON object).
    /// </summary>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!ConsumesMultipart(context))
                return;

            var formParameters = context.ApiDescription.ParameterDescriptions
                .Where(p => p.Source == BindingSource.Form)
                .ToList();

            if (formParameters.Count == 0)
                return;

            var properties = new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase);
            var encoding = new Dictionary<string, OpenApiEncoding>(StringComparer.OrdinalIgnoreCase);

            foreach (var parameter in formParameters)
            {
                var name = parameter.Name;
                if (string.IsNullOrEmpty(name) || properties.ContainsKey(name))
                    continue;

                var modelType = parameter.ModelMetadata?.ModelType ?? typeof(string);
                var underlying = Nullable.GetUnderlyingType(modelType) ?? modelType;

                if (underlying == typeof(IFormFile) || underlying == typeof(IFormFile[]))
                {
                    properties[name] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary",
                        Description = "Optional punch photo (JPG/PNG, max 2 MB)"
                    };
                    encoding[name] = new OpenApiEncoding
                    {
                        Style = ParameterStyle.Form,
                        ContentType = "image/jpeg, image/png"
                    };
                    continue;
                }

                properties[name] = underlying switch
                {
                    _ when underlying == typeof(int) => new OpenApiSchema { Type = "integer", Format = "int32" },
                    _ when underlying == typeof(long) => new OpenApiSchema { Type = "integer", Format = "int64" },
                    _ when underlying == typeof(double) || underlying == typeof(float) => new OpenApiSchema { Type = "number", Format = "double" },
                    _ when underlying == typeof(bool) => new OpenApiSchema { Type = "boolean" },
                    _ when underlying == typeof(DateTime) => new OpenApiSchema { Type = "string", Format = "date-time" },
                    _ => new OpenApiSchema { Type = "string" }
                };

                encoding[name] = new OpenApiEncoding { Style = ParameterStyle.Form };
            }

            if (properties.Count == 0)
                return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties
                        },
                        Encoding = encoding
                    }
                }
            };

            operation.Parameters?.Clear();
        }

        private static bool ConsumesMultipart(OperationFilterContext context)
        {
            return context.ApiDescription.ActionDescriptor.EndpointMetadata
                .OfType<ConsumesAttribute>()
                .Any(a => a.ContentTypes.Any(t =>
                    t.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase)));
        }
    }
}
