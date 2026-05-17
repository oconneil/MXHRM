using Microsoft.AspNetCore.Identity;
using AppPermissions = MXHRM.Application.Authorization.Permissions;
using MXHRM.Infrastructure.Authorization;
using MXHRM.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MXHRM.Infrastructure.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var roles = new[] { "Admin", "HR", "Employee" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        await SeedPermissionsAsync(db);
        await SeedRolePermissionsAsync(db, roleManager);
    }


    private static async Task SeedPermissionsAsync(AppDbContext db)
    {
        var permissionSeeds = new List<Permission>
        {
            new() { Code = AppPermissions.Employee.Read, Name = "Employee Read" },
            new() { Code = AppPermissions.Employee.Create, Name = "Employee Create" },
            new() { Code = AppPermissions.Employee.Update, Name = "Employee Update" },
            new() { Code = AppPermissions.Employee.Delete, Name = "Employee Delete" },
            new() { Code = AppPermissions.Role.Manage, Name = "Role Manage" },
            new() { Code = AppPermissions.Audit.Read, Name = "Audit Read" },
            new() { Code = AppPermissions.Activity.Read, Name = "Activity Read" }
        };

        foreach (var p in permissionSeeds)
        {
            var exists = await db.Permissions.AnyAsync(x => x.Code == p.Code);
            if (!exists) db.Permissions.Add(p);
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedRolePermissionsAsync(
    AppDbContext db,
    RoleManager<IdentityRole> roleManager)
    {
        var adminRole = await roleManager.FindByNameAsync("Admin");
        var hrRole = await roleManager.FindByNameAsync("HR");
        var employeeRole = await roleManager.FindByNameAsync("Employee");

        if (adminRole is null || hrRole is null || employeeRole is null) return;

        var allPermissions = await db.Permissions.ToListAsync();
        int GetId(string code) => allPermissions.First(x => x.Code == code).Id;

        var map = new Dictionary<string, string[]>
        {
            [adminRole.Id] = new[]
            {
                AppPermissions.Employee.Read,
                AppPermissions.Employee.Create,
                AppPermissions.Employee.Update,
                AppPermissions.Employee.Delete,
                AppPermissions.Role.Manage,
                AppPermissions.Audit.Read,
                AppPermissions.Activity.Read
            },
            [hrRole.Id] = new[]
            {
            AppPermissions.Employee.Read,
            AppPermissions.Employee.Create,
            AppPermissions.Employee.Update
            },
            [employeeRole.Id] = new[]
            {
            AppPermissions.Employee.Read
            }
        };

        foreach (var (roleId, permissionCodes) in map)
        {
            foreach (var code in permissionCodes)
            {
                var permissionId = GetId(code);

                var exists = await db.RolePermissions
                    .AnyAsync(x => x.RoleId == roleId && x.PermissionId == permissionId);

                if (!exists)
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleId = roleId,
                        PermissionId = permissionId
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }


}
