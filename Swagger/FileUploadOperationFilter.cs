using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
    /// <summary>
    /// Builds multipart/form-data request bodies from [FromForm] parameters
    /// so Swagger UI shows separate fields, including IFormFile Choose File controls.
    /// </summary>
    /// <remarks>
    /// DateTime form fields include OpenAPI Example values in yyyy-MM-ddTHH:mm:ss format.
    /// Swagger UI limitation: some versions still show the property name as the input
    /// placeholder; the Example (e.g. 2026-07-22T09:15:35) is documented on the schema.
    /// </remarks>
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!ConsumesMultipart(context))
                return;

            // IFormFile properties on a [FromForm] model are reported by the ApiExplorer
            // with BindingSource.FormFile (not BindingSource.Form), so both must be included
            // or the file upload field is dropped from the generated schema.
            var formParameters = context.ApiDescription.ParameterDescriptions
                .Where(p => p.Source == BindingSource.Form || p.Source == BindingSource.FormFile)
                .ToList();

            if (formParameters.Count == 0)
                return;

            var properties = new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase);
            var encoding = new Dictionary<string, OpenApiEncoding>(StringComparer.OrdinalIgnoreCase);

            foreach (var parameter in formParameters)
            {
                var modelMetadata = parameter.ModelMetadata;
                var modelType = modelMetadata?.ModelType ?? parameter.Type ?? typeof(string);
                var underlying = Nullable.GetUnderlyingType(modelType) ?? modelType;

                // Expand complex [FromForm] models (e.g. PunchInRequest) into form fields.
                // IFormFile and IFormFile collections must NOT be expanded (a List<IFormFile>
                // otherwise leaks its Capacity/Count properties instead of a file field).
                if (modelMetadata?.Properties is { Count: > 0 } children &&
                    underlying != typeof(IFormFile) &&
                    !IsFormFileCollection(underlying) &&
                    underlying != typeof(string) &&
                    !underlying.IsPrimitive &&
                    underlying != typeof(decimal) &&
                    underlying != typeof(DateTime) &&
                    underlying != typeof(Guid))
                {
                    ExpandComplexFormModel(properties, encoding, underlying, modelMetadata);
                    continue;
                }

                var name = parameter.Name;
                if (string.IsNullOrEmpty(name) || properties.ContainsKey(name))
                    continue;

                AddFormProperty(properties, encoding, name, modelType);
            }

            // Safety net: if ApiExplorer omitted IFormFile children, add them via reflection.
            EnsureFilePropertiesFromParameterTypes(properties, encoding, formParameters);

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

            // Keep path/header parameters (e.g. the PUT {id} route value); only the
            // query/form parameters are replaced by the multipart request body above.
            if (operation.Parameters != null)
            {
                operation.Parameters = operation.Parameters
                    .Where(p => p.In == ParameterLocation.Path || p.In == ParameterLocation.Header)
                    .ToList();
            }
        }

        private static bool IsComplexFormModel(Type underlying, ModelMetadata? modelMetadata)
        {
            if (underlying == typeof(IFormFile) ||
                underlying == typeof(IFormFile[]) ||
                underlying == typeof(string) ||
                underlying.IsPrimitive ||
                underlying == typeof(decimal) ||
                underlying == typeof(DateTime) ||
                underlying == typeof(Guid) ||
                underlying == typeof(bool))
            {
                return false;
            }

            return (modelMetadata?.Properties?.Count ?? 0) > 0 ||
                   underlying.GetProperties(BindingFlags.Instance | BindingFlags.Public).Length > 0;
        }

        private static void ExpandComplexFormModel(
            IDictionary<string, OpenApiSchema> properties,
            IDictionary<string, OpenApiEncoding> encoding,
            Type modelType,
            ModelMetadata? modelMetadata)
        {
            if (modelMetadata?.Properties is { Count: > 0 } children)
            {
                foreach (var child in children)
                {
                    AddFormProperty(
                        properties,
                        encoding,
                        child.BinderModelName ?? child.PropertyName ?? child.Name,
                        child.ModelType);
                }
            }

            // Reflection ensures IFormFile properties are present even when ModelMetadata omits them.
            foreach (var property in modelType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead)
                    continue;

                AddFormProperty(properties, encoding, property.Name, property.PropertyType);
            }
        }

        private static void EnsureFilePropertiesFromParameterTypes(
            IDictionary<string, OpenApiSchema> properties,
            IDictionary<string, OpenApiEncoding> encoding,
            IReadOnlyList<Microsoft.AspNetCore.Mvc.ApiExplorer.ApiParameterDescription> formParameters)
        {
            foreach (var parameter in formParameters)
            {
                var type = parameter.ModelMetadata?.ModelType ?? parameter.Type;
                if (type == null)
                    continue;

                var underlying = Nullable.GetUnderlyingType(type) ?? type;
                if (underlying == typeof(IFormFile) || underlying == typeof(IFormFile[]))
                {
                    AddFormProperty(properties, encoding, parameter.Name, underlying);
                    continue;
                }

                if (underlying.IsPrimitive || underlying == typeof(string) || underlying == typeof(DateTime))
                    continue;

                foreach (var property in underlying.GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    if (propertyType == typeof(IFormFile) || propertyType == typeof(IFormFile[]))
                        AddFormProperty(properties, encoding, property.Name, property.PropertyType);
                }
            }
        }

        private static void AddFormProperty(
            IDictionary<string, OpenApiSchema> properties,
            IDictionary<string, OpenApiEncoding> encoding,
            string? name,
            Type modelType)
        {
            if (string.IsNullOrEmpty(name) || properties.ContainsKey(name))
                return;

            var underlying = Nullable.GetUnderlyingType(modelType) ?? modelType;

            if (IsFormFileCollection(underlying))
            {
                properties[name] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema { Type = "string", Format = "binary" },
                    Description = "One or more files to upload."
                };
                encoding[name] = new OpenApiEncoding { Style = ParameterStyle.Form };
                return;
            }

            if (underlying == typeof(IFormFile))
            {
                properties[name] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary",
                    Nullable = true,
                    Description = "Optional punch photo (JPG/PNG, max 2 MB)"
                };
                encoding[name] = new OpenApiEncoding
                {
                    Style = ParameterStyle.Form,
                    ContentType = "application/octet-stream"
                };
                return;
            }

            properties[name] = underlying switch
            {
                _ when underlying == typeof(int) => new OpenApiSchema { Type = "integer", Format = "int32" },
                _ when underlying == typeof(long) => new OpenApiSchema { Type = "integer", Format = "int64" },
                _ when underlying == typeof(double) || underlying == typeof(float) => new OpenApiSchema { Type = "number", Format = "double" },
                _ when underlying == typeof(bool) => new OpenApiSchema { Type = "boolean" },
                _ when underlying == typeof(DateTime) => CreateDateTimeSchema(name),
                _ => new OpenApiSchema { Type = "string" }
            };

            encoding[name] = new OpenApiEncoding { Style = ParameterStyle.Form };
        }

        private static OpenApiSchema CreateDateTimeSchema(string propertyName)
        {
            var example = AttendanceDateTimeSwaggerExamples.Resolve(propertyName);
            return new OpenApiSchema
            {
                Type = "string",
                Format = "date-time",
                Example = AttendanceDateTimeSwaggerExamples.ToOpenApiString(propertyName),
                Description = $"Format: {AttendanceDateTimeSwaggerExamples.FormatHint}. Example: {example}"
            };
        }

        private static bool IsFormFileCollection(Type type)
        {
            if (type == typeof(IFormFileCollection))
                return true;

            if (type.IsArray)
                return type.GetElementType() == typeof(IFormFile);

            if (type.IsGenericType)
                return type.GetGenericArguments().FirstOrDefault() == typeof(IFormFile);

            return false;
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
