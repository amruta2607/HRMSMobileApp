using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Adds dual request examples on the login-email operation (username and email variants).
	/// </summary>
	public sealed class LoginRequestOperationFilter : IOperationFilter
	{
		public void Apply(OpenApiOperation operation, OperationFilterContext context)
		{
			var relativePath = context.ApiDescription.RelativePath ?? string.Empty;
			if (!relativePath.Contains("login-email", StringComparison.OrdinalIgnoreCase))
				return;

			if (operation.RequestBody?.Content == null)
				return;

			foreach (var mediaType in operation.RequestBody.Content.Values)
			{
				mediaType.Examples = new Dictionary<string, OpenApiExample>
				{
					["Username"] = new OpenApiExample
					{
						Summary = "Login with username",
						Value = new OpenApiObject
						{
							["usernameOrEmail"] = new OpenApiString("john.doe"),
							["password"] = new OpenApiString("******")
						}
					},
					["Email"] = new OpenApiExample
					{
						Summary = "Login with email",
						Value = new OpenApiObject
						{
							["usernameOrEmail"] = new OpenApiString("john@company.com"),
							["password"] = new OpenApiString("******")
						}
					}
				};
			}

			operation.Summary ??= "Login with username or email and password";
			operation.Description =
				"Authenticate using either Username or Email in usernameOrEmail. " +
				"Legacy clients may still send email instead of usernameOrEmail. " +
				"Failures always return a generic invalid credentials message.";
		}
	}
}
