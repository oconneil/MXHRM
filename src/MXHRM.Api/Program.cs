using FluentValidation;
using FluentValidation.AspNetCore;
using MXHRM.Api.Authorization;
using MXHRM.Api.Middlewares;
using MXHRM.Api.Common;
using Serilog;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application;
using MXHRM.Infrastructure;
using MXHRM.Infrastructure.Data;
using Hangfire;
using MXHRM.Infrastructure.Jobs;
using Hangfire.Dashboard;
using MXHRM.Api.Hangfire;
using MXHRM.Api.Services;
using MXHRM.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using MXHRM.Api.Swagger;
using MXHRM.Api.Hubs;
using MXHRM.Application.Common.Realtime;
using Microsoft.AspNetCore.SignalR;
using MXHRM.Api.SignalR;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Identity;
using MXHRM.Infrastructure.Identity;
using System.Security.Claims;
using MXHRM.Infrastructure.Auth;

// Create the WebApplication builder
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Serilog for logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Register the current user service to access user information in services and controllers
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
// Register the tenant provider to access the current tenant (company) information
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
// Register the SignalR real-time notifier for sending real-time updates to clients
builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

builder.Services.AddControllers();

// Configure forwarded headers to correctly handle client IP and protocol when behind a reverse proxy
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;

    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Add SignalR services and configure the user ID provider for SignalR to use the current user's ID
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

// Configure custom error response for model validation errors
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                x => x.Key,
                x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

        var response = new ErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Code = ErrorCodes.ValidationError,
            Message = "Validation failed.",
            Details = errors,
            TraceId = context.HttpContext.TraceIdentifier
        };

        return new BadRequestObjectResult(response);
    };
});

// Add FluentValidation services
builder.Services.AddFluentValidationAutoValidation();

// Register validators
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);

// Register infrastructure services (EF Core, Identity, and use-case implementations)
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with JWT authentication support
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<FileResponseOperationFilter>();
    options.CustomOperationIds(apiDescription =>
    {
        if (apiDescription.ActionDescriptor is ControllerActionDescriptor actionDescriptor)
        {
            return $"{actionDescriptor.ControllerName}_{actionDescriptor.ActionName}";
        }

        return null;
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"];

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
    {
        // Set the default authentication scheme to JWT Bearer
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        // Configure JWT Bearer options
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecretKey!)),

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Custom events for handling authentication failures and forbidden responses
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrWhiteSpace(accessToken) &&
                    path.StartsWithSegments("/hubs/realtime"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },

            OnTokenValidated = async context =>
            {
                var cache = context.HttpContext.RequestServices
                    .GetRequiredService<ICacheService>();

                var userId = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? context.Principal?.FindFirst("sub")?.Value;
                var tokenStamp = context.Principal?.FindFirst("security_stamp")?.Value;

                if (userId is null)
                {
                    context.Fail("Token is no longer valid.");
                    return;
                }

                var cacheKey = AuthCacheKeys.UserSecurity(userId);
                var snapshot = await cache.GetAsync<UserSecuritySnapshot>(cacheKey);

                if (snapshot is null)
                {
                    // cache miss → โหลด DB ครั้งเดียว แล้วเก็บลง Redis (TTL 5 นาที = safety net)
                    var userManager = context.HttpContext.RequestServices
                        .GetRequiredService<UserManager<ApplicationUser>>();
                    var user = await userManager.FindByIdAsync(userId);

                    if (user is null)
                    {
                        context.Fail("Token is no longer valid.");
                        return;
                    }

                    snapshot = new UserSecuritySnapshot(user.SecurityStamp ?? string.Empty, user.IsActive);
                    await cache.SetAsync(cacheKey, snapshot, TimeSpan.FromMinutes(5));
                }

                // token ใช้ไม่ได้ถ้า: user ถูกปิด / stamp ไม่ตรง (สิทธิ์เปลี่ยนแล้ว)
                if (!snapshot.IsActive ||
                    !string.Equals(snapshot.SecurityStamp, tokenStamp, StringComparison.Ordinal))
                {
                    context.Fail("Token is no longer valid.");
                }
            },

            OnChallenge = async context =>
            {
                context.HandleResponse();

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                var response = new ErrorResponse
                {
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Code = ErrorCodes.Unauthorized,
                    Message = "Unauthorized.",
                    TraceId = context.HttpContext.TraceIdentifier
                };

                await context.Response.WriteAsJsonAsync(response);
            },

            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var response = new ErrorResponse
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Code = ErrorCodes.Forbidden,
                    Message = "You do not have permission to perform this action.",
                    TraceId = context.HttpContext.TraceIdentifier
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        };

    });

// Register the custom authorization handler for permissions
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
// Register the custom authorization handler for same-company access control
builder.Services.AddSingleton<IAuthorizationHandler, SameCompanyAuthorizationHandler>();
// Register the custom authorization policy provider to dynamically create policies based on permissions
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
// Add authorization services
builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularDev", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});

// Configure rate limiting
builder.Services.AddRateLimiter(options =>
{
    // policy "login": นับต่อ IP, 5 ครั้ง/นาที
    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // ตอบ 429 ด้วย ErrorResponse contract เดิม
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString();
        }

        var response = new ErrorResponse
        {
            StatusCode = StatusCodes.Status429TooManyRequests,
            Code = ErrorCodes.TooManyRequests,
            Message = "พยายามเข้าสู่ระบบบ่อยเกินไป กรุณารอสักครู่แล้วลองใหม่",
            TraceId = context.HttpContext.TraceIdentifier
        };

        await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    };
});

var app = builder.Build();

// Enable forwarded headers middleware to process X-Forwarded-For and X-Forwarded-Proto headers
app.UseForwardedHeaders();

// Add Serilog request logging middleware
app.UseSerilogRequestLogging();

// Global exception handling middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Enable Swagger in development environment
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AngularDev");

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();
// Enable rate limiting
app.UseRateLimiter();

// Get the Hangfire dashboard path from configuration, with a default fallback
var hangfireDashboardPath = builder.Configuration["Hangfire:DashboardPath"] ?? "/hangfire";
// Configure Hangfire dashboard with authorization in production, and open access in development
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard(hangfireDashboardPath);
}
else
{
    app.UseHangfireDashboard(
        hangfireDashboardPath,
        new DashboardOptions
        {
            Authorization = new[]
            {
                new HangfireDashboardAuthorizationFilter()
            }
        });
}

// Map controllers
app.MapControllers();

// Map a lightweight health endpoint for Docker and reverse proxy checks
app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "MXHRM.Api",
    checkedAtUtc = DateTime.UtcNow
}));

// Map the SignalR hub for real-time communication
app.MapHub<RealtimeHub>("/hubs/realtime");

// In the Testing environment we skip infra-coupled startup side effects
// (DB seeding + Hangfire recurring jobs) so WebApplicationFactory can boot the app
// without a real SQL Server. This is what makes the API integration-testable.
if (!app.Environment.IsEnvironment("Testing"))
{
    // Seed initial data (roles and admin user)
    await IdentitySeeder.SeedRolesAsync(app.Services);

    // Configure recurring jobs
    var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<SystemHealthJob>(
        "system-health-job",            // Unique ID for the job
        job => job.ExecuteAsync(),      // method to execute
        Cron.Minutely);                 // Run every minute for demonstration; adjust as needed in production

    // Get the cron expression for the refresh token cleanup job from configuration, with a default fallback
    var refreshTokenCleanupCron = builder.Configuration["RefreshTokenCleanup:Cron"]
        ?? Cron.Daily(2); // Default to daily at 2 AM if not configured
    recurringJobManager.AddOrUpdate<CleanupExpiredRefreshTokensJob>(
        "cleanup-expired-refresh-tokens",   // Unique ID for the job
        job => job.ExecuteAsync(),          // method to execute
        refreshTokenCleanupCron);           // Run based on the configured cron expression (default is daily at 2 AM)

    // Get the cron expression for the employee report job from configuration, with a default fallback
    var employeeReportCron = builder.Configuration["EmployeeReport:Cron"]
        ?? Cron.Daily(6);
    recurringJobManager.AddOrUpdate<EmployeeReportJob>(
        "employee-summary-report",
        job => job.ExecuteAsync(),
        employeeReportCron);
}

app.Run();

// Expose the implicit Program class to the integration-test project (WebApplicationFactory<Program>)
public partial class Program;
