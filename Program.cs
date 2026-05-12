using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using System.Text;
using System.Linq;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Repositories;
using MobileWebApi.Services;
using MobileWebApi.Models;
using MobileWebApi.Middleware;
using MobileWebApi.Swagger;
using Serilog;
using MobileWebApi.Helper;
using MobileWebApi.Resources;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Serilog Setup
// ----------------------
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.File("C:/Logs/SampleApiLogs/log.txt", rollingInterval: RollingInterval.Day)
	.CreateLogger();

builder.Host.UseSerilog();

// ----------------------
// Configuration
// ----------------------
builder.Configuration
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("Resources/queries.json", optional: false, reloadOnChange: true);
builder.Services.AddSingleton<QueryProvider>();

// ----------------------
// Register HttpContext & Tenant
// ----------------------
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, TenantContext>();

// ----------------------
// Register services
// ----------------------
builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<LocationRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<BlobService>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();

builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
builder.Services.AddScoped<IHolidayService, HolidayService>();

builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();

builder.Services.AddScoped<IPaySlipRepository, PaySlipRepository>();
builder.Services.AddScoped<IPaySlipService, PaySlipService>();

builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
builder.Services.AddScoped<IDisputeService, DisputeService>();
builder.Services.AddScoped<ITenantConfigurationRepository, TenantConfigurationRepository>();
builder.Services.AddScoped<IGeoTenantLocationRepository, GeoTenantLocationRepository>();
builder.Services.AddScoped<IMobileTenantConfigurationRepository, MobileTenantConfigurationRepository>();
builder.Services.AddScoped<IMobileModuleAccessService, MobileModuleAccessService>();

builder.Services.AddSingleton<ISqlConnections, MobileWebApi.Data.DefaultSqlConnections>();
builder.Services.AddScoped<IAttendanceOverviewService, AttendanceOverviewService>();

builder.Services.AddScoped<IMobileDashboardService, MobileDashboardService>();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IOtpService, OtpService>();
builder.Services.AddHttpClient();

// Background cleanup for punch images.
builder.Services.AddHostedService<BlobCleanupService>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
builder.Services.AddScoped<ISmsService, SmsService>();

builder.Services.Configure<PinKeySetting>(builder.Configuration.GetSection("PinEncryption"));

// ----------------------
// Controllers & Swagger
// ----------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo
	{
		Title = "Sample Web API",
		Version = "v1",
		Description = "A simple JWT-secured Web API"
	});

	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Name = "Authorization",
		Type = SecuritySchemeType.Http,
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description = "Enter 'Bearer' followed by your JWT token. Ex: Bearer abc.xyz.123"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
			},
			Array.Empty<string>()
		}
	});

	c.OperationFilter<HideMobileDashboardResponseSchemaFilter>();
});

// ----------------------
// JWT Authentication
// ----------------------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];
if (string.IsNullOrWhiteSpace(secretKey))
{
	throw new InvalidOperationException(
		"Jwt:Key is missing or empty. Set Jwt:Key in appsettings.json, use dotnet user-secrets set \"Jwt:Key\" \"<your-secret>\", or set environment variable Jwt__Key. Use at least 32 characters.");
}

var jwtKeyBytes = Encoding.UTF8.GetBytes(secretKey);
if (jwtKeyBytes.Length < 32)
{
	throw new InvalidOperationException(
		"Jwt:Key must be at least 32 bytes when UTF-8 encoded (Microsoft.IdentityModel recommends a sufficiently long symmetric key for HMAC-SHA256).");
}

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = jwtSettings["Issuer"],
		ValidAudience = jwtSettings["Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(jwtKeyBytes)
	};

	options.Events = new JwtBearerEvents
	{
		OnTokenValidated = async context =>
		{
			var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
			var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
			if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
			{
				var token = authHeader.Substring("Bearer ".Length).Trim();
				if (!string.IsNullOrEmpty(token) && tokenService.IsTokenBlacklisted(token))
					context.Fail("Token has been revoked. Please login again.");
			}

			await Task.CompletedTask;
		}
	};
});

// ----------------------
// Register Upload Folder BEFORE builder.Build()
// ----------------------
var uploadPath = builder.Configuration["UploadSettings:Path"];
if (!string.IsNullOrWhiteSpace(uploadPath))
{
	var fullUploadPath = Path.GetFullPath(uploadPath);
	if (!Directory.Exists(fullUploadPath))
		Directory.CreateDirectory(fullUploadPath);

	// Register as singleton so services can use it
	builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(fullUploadPath));
}

// ----------------------
// Build App
// ----------------------
var app = builder.Build();

// ----------------------
// Middleware pipeline
// ----------------------
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Tenant middleware
app.UseTenantAccessValidation();

// Serve wwwroot
app.UseStaticFiles();

// Serve upload folder
if (!string.IsNullOrWhiteSpace(uploadPath))
{
	var fullUploadPath = Path.GetFullPath(uploadPath);
	app.UseStaticFiles(new StaticFileOptions
	{
		FileProvider = new PhysicalFileProvider(fullUploadPath),
		RequestPath = "/upload"
	});
}

app.MapControllers();
app.Run();
