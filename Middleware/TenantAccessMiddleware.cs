using MobileWebApi.Services;
using MobileWebApi.Constants;
using System.Text.Json;

namespace MobileWebApi.Middleware
{
    /// <summary>
    /// Middleware that handles TenantAccessException globally.
    /// Returns a 403 Forbidden response when tenant access is denied.
    /// </summary>
    public class TenantAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantAccessMiddleware> _logger;

        public TenantAccessMiddleware(RequestDelegate next, ILogger<TenantAccessMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (TenantAccessException ex)
            {
                _logger.LogWarning(ex, LogMessages.Middleware.TenantAccessViolationDetected);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = new
                {
                    Success = false,
                    Message = "Access denied: You can only access data from your own organisation.",
                    Error = "TENANT_ACCESS_DENIED"
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }

    /// <summary>
    /// Extension method to register the TenantAccessMiddleware
    /// </summary>
    public static class TenantAccessMiddlewareExtensions
    {
        public static IApplicationBuilder UseTenantAccessValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantAccessMiddleware>();
        }
    }
}

