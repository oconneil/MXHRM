using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MXHRM.Application.Auth;
using MXHRM.Application.Employees;
using MXHRM.Application.Permissions;
using MXHRM.Application.Roles;
using MXHRM.Application.Users;
using MXHRM.Infrastructure.Auth;
using MXHRM.Infrastructure.Data;
using MXHRM.Infrastructure.Employees;
using MXHRM.Infrastructure.Identity;
using MXHRM.Infrastructure.Permissions;
using MXHRM.Infrastructure.Roles;
using MXHRM.Infrastructure.Users;

namespace MXHRM.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

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

        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
