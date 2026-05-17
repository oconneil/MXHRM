using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MXHRM.Application.Auth;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Application.Employees;
using MXHRM.Application.Permissions;
using MXHRM.Application.Roles;
using MXHRM.Application.Users;
using MXHRM.Infrastructure.Auth;
using MXHRM.Infrastructure.Caching;
using MXHRM.Infrastructure.Data;
using MXHRM.Infrastructure.Employees;
using MXHRM.Infrastructure.Identity;
using MXHRM.Infrastructure.Permissions;
using MXHRM.Infrastructure.Roles;
using MXHRM.Infrastructure.Users;
using StackExchange.Redis;
using Hangfire;
using Hangfire.SqlServer;
using MXHRM.Infrastructure.Jobs;
using MXHRM.Application.Auditing;
using MXHRM.Infrastructure.Auditing;

namespace MXHRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(defaultConnectionString))
        {
            throw new InvalidOperationException("Default database connection string is not configured.");
        }

        services.AddHangfire(config =>
        {
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(defaultConnectionString, new SqlServerStorageOptions
                {
                    SchemaName = configuration["Hangfire:SchemaName"] ?? "Hangfire",
                    PrepareSchemaIfNecessary = true
                });
        });

        services.AddHangfireServer();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration["Redis:ConnectionString"];
            options.InstanceName = "MXHRM:";
        });

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var redisConnectionString = configuration["Redis:ConnectionString"];

            if (string.IsNullOrWhiteSpace(redisConnectionString))
            {
                throw new InvalidOperationException("Redis connection string is not configured.");
            }

            return ConnectionMultiplexer.Connect(redisConnectionString);
        });


        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;

                // MXHRM supports the same email across different companies.
                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        // Register application services
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<SystemHealthJob>();
        services.AddScoped<CleanupExpiredRefreshTokensJob>();
        services.AddScoped<EmployeeReportJob>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IUserActivityLogService, UserActivityLogService>();

        return services;
    }
}
