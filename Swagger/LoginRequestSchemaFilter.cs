using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MobileWebApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MobileWebApi.Swagger
{
	/// <summary>
	/// Documents login requests with usernameOrEmail examples (username and email variants).
	/// Legacy email / Username fields remain accepted by the API but are marked deprecated in Swagger.
	/// </summary>
	public sealed class LoginRequestSchemaFilter : ISchemaFilter
	{
		public void Apply(OpenApiSchema schema, SchemaFilterContext context)
		{
			if (context.Type == typeof(EmailLoginRequest))
			{
				ApplyEmailLoginSchema(schema);
				return;
			}

			if (context.Type == typeof(WebLoginRequest))
			{
				ApplyWebLoginSchema(schema);
			}
		}

		private static void ApplyEmailLoginSchema(OpenApiSchema schema)
		{
			schema.Description =
				"Authenticate with username or email and password. Prefer usernameOrEmail; email is accepted for backward compatibility.";

			schema.Required ??= new HashSet<string>();
			schema.Required.Add("usernameOrEmail");
			schema.Required.Add("password");
			schema.Required.Remove("email");
			schema.Required.Remove("ResolvedUsernameOrEmail");

			schema.Properties ??= new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase);
			schema.Properties.Remove("ResolvedUsernameOrEmail");

			schema.Properties["usernameOrEmail"] = new OpenApiSchema
			{
				Type = "string",
				Description = "Username or email address (case-insensitive). Examples: john.doe or john@company.com",
				Example = new OpenApiString("john.doe")
			};

			schema.Properties["password"] = new OpenApiSchema
			{
				Type = "string",
				Format = "password",
				Description = "Account password.",
				Example = new OpenApiString("******")
			};

			if (schema.Properties.TryGetValue("email", out var emailSchema))
			{
				emailSchema.Description =
					"Deprecated. Use usernameOrEmail. Still accepted for backward compatibility.";
				emailSchema.Deprecated = true;
				emailSchema.Example = new OpenApiString("john@company.com");
			}

			// Primary documented example (username). Email example is covered via property description.
			schema.Example = new OpenApiObject
			{
				["usernameOrEmail"] = new OpenApiString("john.doe"),
				["password"] = new OpenApiString("******")
			};
		}

		private static void ApplyWebLoginSchema(OpenApiSchema schema)
		{
			schema.Description =
				"Web login with username or email and password. Prefer UsernameOrEmail; Username is accepted for backward compatibility.";

			schema.Required ??= new HashSet<string>();
			schema.Required.Add("UsernameOrEmail");
			schema.Required.Add("Password");
			schema.Required.Remove("Username");
			schema.Required.Remove("ResolvedUsernameOrEmail");

			schema.Properties ??= new Dictionary<string, OpenApiSchema>(StringComparer.OrdinalIgnoreCase);
			schema.Properties.Remove("ResolvedUsernameOrEmail");

			schema.Properties["UsernameOrEmail"] = new OpenApiSchema
			{
				Type = "string",
				Description = "Username or email address (case-insensitive). Examples: john.doe or john@company.com",
				Example = new OpenApiString("john.doe")
			};

			schema.Properties["Password"] = new OpenApiSchema
			{
				Type = "string",
				Format = "password",
				Description = "Account password.",
				Example = new OpenApiString("******")
			};

			if (schema.Properties.TryGetValue("Username", out var usernameSchema))
			{
				usernameSchema.Description =
					"Deprecated. Use UsernameOrEmail. Still accepted for backward compatibility.";
				usernameSchema.Deprecated = true;
			}

			schema.Example = new OpenApiObject
			{
				["UsernameOrEmail"] = new OpenApiString("john@company.com"),
				["Password"] = new OpenApiString("******")
			};
		}
	}
}
