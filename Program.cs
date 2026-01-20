using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Linq;
using MobileWebApi.Data;
using MobileWebApi.Interfaces;
using MobileWebApi.Repositories;
using MobileWebApi.Services;
using MobileWebApi.Models;
using MobileWebApi.Middleware;
using Serilog;
using MobileWebApi.Helper;
using MobileWebApi.Resources;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Load encryption config
//PinEncryptionHelper.Init(builder.Configuration);

// Serilog setup
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.File("C:/Logs/SampleApiLogs/log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Load configuration
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();
builder.Configuration.AddJsonFile("Resources/queries.json", optional: false, reloadOnChange: true);
builder.Services.AddSingleton<QueryProvider>();

// Register HttpContextAccessor for tenant context
builder.Services.AddHttpContextAccessor();

// Register Tenant Context for multi-tenant isolation
builder.Services.AddScoped<ITenantContext, TenantContext>();

// Register services
builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<LocationRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// Alert/Notification services
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IAlertService, AlertService>();

// Leave Management services
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();
builder.Services.AddScoped<ILeaveService, LeaveService>();

// Holiday Management services
builder.Services.AddScoped<IHolidayRepository, HolidayRepository>();
builder.Services.AddScoped<IHolidayService, HolidayService>();

// Approval Workflow services
builder.Services.AddScoped<IApprovalRepository, ApprovalRepository>();
builder.Services.AddScoped<IApprovalWorkflowService, ApprovalWorkflowService>();

// Pay Slip services
builder.Services.AddScoped<IPaySlipRepository, PaySlipRepository>();
builder.Services.AddScoped<IPaySlipService, PaySlipService>();

// Dispute Management services
builder.Services.AddScoped<IDisputeRepository, DisputeRepository>();
builder.Services.AddScoped<IDisputeService, DisputeService>();

// ISqlConnections for database connection management
builder.Services.AddSingleton<ISqlConnections, MobileWebApi.Data.DefaultSqlConnections>();

// Attendance Overview services
builder.Services.AddScoped<IAttendanceOverviewService, AttendanceOverviewService>();

// OTP Service for forgot password and mobile login
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IOtpService, OtpService>();

// Email Service for forgot password
builder.Services.Configure<MobileWebApi.Models.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// SMS Service for OTP
builder.Services.Configure<MobileWebApi.Models.SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
builder.Services.AddScoped<ISmsService, SmsService>();

// Register Pin Encryption
builder.Services.Configure<PinKeySetting>(builder.Configuration.GetSection("PinEncryption"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
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
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["Key"];

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    // Validate token blacklist on each request
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();
            
            // Extract the actual token string from the Authorization header
            var authHeader = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                
                if (!string.IsNullOrEmpty(token) && tokenService.IsTokenBlacklisted(token))
                {
                    context.Fail("Token has been revoked. Please login again.");
                }
            }
            
            await Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Configure static files to serve images from wwwroot
app.UseStaticFiles();

// Configure static files to serve images from shared upload folder
// This allows both Serenity web app and Web API to access images from the same shared location
var rootPath = builder.Configuration["UploadSettings:Path"];
if (!string.IsNullOrWhiteSpace(rootPath))
{
    // Normalize the absolute path (handles both forward and backslashes, trailing slashes, etc.)
    var fullUploadPath = Path.GetFullPath(rootPath);
    
    // Ensure directory exists
    if (!Directory.Exists(fullUploadPath))
    {
        Directory.CreateDirectory(fullUploadPath);
    }
    
    var staticFileOptions = new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(fullUploadPath),
        // Map /upload URL path to the configured upload physical path
        // So /upload/Image/Employee/00000/file.jpg serves from the configured upload path
        RequestPath = "/upload"
    };
    
    app.UseStaticFiles(staticFileOptions);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Add tenant access validation middleware - handles TenantAccessException globally
app.UseTenantAccessValidation();

app.MapControllers();
app.Run();
