using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using MXHRM.Api.Common;
using MXHRM.Api.Authorization;
using MXHRM.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MXHRM.Api.Data;

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
        new() { Code = Permissions.Employee.Read, Name = "Employee Read" },
        new() { Code = Permissions.Employee.Create, Name = "Employee Create" },
        new() { Code = Permissions.Employee.Update, Name = "Employee Update" },
        new() { Code = Permissions.Employee.Delete, Name = "Employee Delete" },
        new() { Code = Permissions.Role.Manage, Name = "Role Manage" }
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
            Permissions.Employee.Read,
            Permissions.Employee.Create,
            Permissions.Employee.Update,
            Permissions.Employee.Delete,
            Permissions.Role.Manage
        },
            [hrRole.Id] = new[]
            {
            Permissions.Employee.Read,
            Permissions.Employee.Create,
            Permissions.Employee.Update
        },
            [employeeRole.Id] = new[]
            {
            Permissions.Employee.Read
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