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

builder.Services.AddControllers();

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
            .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

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

app.Run();
