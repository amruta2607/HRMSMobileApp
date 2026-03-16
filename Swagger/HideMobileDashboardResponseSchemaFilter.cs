using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
    /// <summary>
    /// Removes the response body schema and example from mobile dashboard GET endpoints in Swagger UI.
    /// </summary>
    public class HideMobileDashboardResponseSchemaFilter : IOperationFilter
    {
        private static readonly HashSet<string> MobileDashboardPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/mobile/training",
            "/api/mobile/announcements",
            "/api/mobile/events",
            "/api/mobile/holidays"
        };

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var path = context.ApiDescription.RelativePath;
            if (path == null || !MobileDashboardPaths.Contains("/" + path.TrimStart('/')))
                return;

            if (operation.Responses.TryGetValue("200", out var response))
                response.Content?.Clear();
        }
    }
}
