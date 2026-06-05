using Microsoft.AspNetCore.Identity;
using AppPermissions = MXHRM.Application.Authorization.Permissions;
using MXHRM.Infrastructure.Authorization;
using MXHRM.Infrastructure.Identity;
using MXHRM.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MXHRM.Infrastructure.Data;

public static class IdentitySeeder
{
    private const string AdminRoleName = "Admin";
    private const string AdminUserName = "admin";
    private const string AdminPassword = "P@ssw0rd";
    private const string AdminCompanyID = "JCORP";

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
        await SeedAdminUserAsync(userManager);
        await SeedDemoUsersAsync(userManager);
        await SeedEmployeesAsync(db);
    }

    // เพิ่ม 3 user (1 admin, 2 employee) ทุกคน password = P@ssw0rd / idempotent (มีแล้วข้าม)
    private static async Task SeedDemoUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var demoUsers = new[]
        {
            new { UserName = "admin.jcorp", Role = "Admin",    CompanyID = "JCORP",  DisplayName = "JCORP Admin" },
            new { UserName = "emp.jamore",  Role = "Employee", CompanyID = "JAMORE", DisplayName = "Jamore Employee" },
            new { UserName = "emp.jcorp",   Role = "Employee", CompanyID = "JCORP",  DisplayName = "JCorp Employee" }
        };

        foreach (var u in demoUsers)
        {
            if (await userManager.FindByNameAsync(u.UserName) is not null) continue;

            var user = new ApplicationUser
            {
                UserName = u.UserName,
                Email = $"{u.UserName}@mxhrm.local",
                EmailConfirmed = true,
                CompanyID = u.CompanyID,
                DisplayName = u.DisplayName,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, "P@ssw0rd");
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to seed user {u.UserName}. {errors}");
            }

            await userManager.AddToRoleAsync(user, u.Role);
        }
    }

    // เพิ่ม Employee 7 คน: JAMORE 3 / JCORP 4 / idempotent (เช็คด้วย CompanyID + EmployeeID)
    private static async Task SeedEmployeesAsync(AppDbContext db)
    {
        var employees = new List<Employee>
        {
            new() { CompanyID = "JAMORE", EmployeeID = "E001", FirstName = "Anan",    LastName = "Jamsai",  Email = "anan@jamore.local",    HireDate = new DateTime(2023, 1, 10), Salary = 35000m },
            new() { CompanyID = "JAMORE", EmployeeID = "E002", FirstName = "Busara",  LastName = "Korn",    Email = "busara@jamore.local",  HireDate = new DateTime(2023, 3, 5),  Salary = 42000m },
            new() { CompanyID = "JAMORE", EmployeeID = "E003", FirstName = "Chai",    LastName = "Wong",    Email = "chai@jamore.local",    HireDate = new DateTime(2024, 6, 1),  Salary = 38000m },
            new() { CompanyID = "JCORP",  EmployeeID = "E001", FirstName = "Daranee", LastName = "Sup",     Email = "daranee@jcorp.local",  HireDate = new DateTime(2022, 11, 20), Salary = 50000m },
            new() { CompanyID = "JCORP",  EmployeeID = "E002", FirstName = "Ekapong", LastName = "Lim",     Email = "ekapong@jcorp.local",  HireDate = new DateTime(2023, 7, 15), Salary = 47000m },
            new() { CompanyID = "JCORP",  EmployeeID = "E003", FirstName = "Fah",     LastName = "Petch",   Email = "fah@jcorp.local",      HireDate = new DateTime(2024, 2, 2),  Salary = 41000m },
            new() { CompanyID = "JCORP",  EmployeeID = "E004", FirstName = "Goong",   LastName = "Mani",    Email = "goong@jcorp.local",    HireDate = new DateTime(2024, 9, 9),  Salary = 39000m }
        };

        foreach (var e in employees)
        {
            var exists = await db.Employees
                .AnyAsync(x => x.CompanyID == e.CompanyID && x.EmployeeID == e.EmployeeID);

            if (!exists)
            {
                e.CreatedBy = "seeder";
                e.ModifiedBy = "seeder";
                db.Employees.Add(e);
            }
        }

        await db.SaveChangesAsync();
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
            new() { Code = AppPermissions.Activity.Read, Name = "Activity Read" },
            new() { Code = AppPermissions.Report.Manage, Name = "Report Manage" }
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
                AppPermissions.Activity.Read,
                AppPermissions.Report.Manage
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

    private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
    {
        var adminUser = await userManager.FindByNameAsync(AdminUserName);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = AdminUserName,
                Email = "admin@mxhrm.local",
                EmailConfirmed = true,
                CompanyID = AdminCompanyID,
                DisplayName = "System Administrator",
                IsActive = true
            };

            var createResult = await userManager.CreateAsync(adminUser, AdminPassword);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to seed admin user. {errors}");
            }
        }
        else
        {
            var hasChanges = false;

            if (adminUser.CompanyID != AdminCompanyID)
            {
                adminUser.CompanyID = AdminCompanyID;
                hasChanges = true;
            }

            if (adminUser.DisplayName != "System Administrator")
            {
                adminUser.DisplayName = "System Administrator";
                hasChanges = true;
            }

            if (!adminUser.IsActive)
            {
                adminUser.IsActive = true;
                hasChanges = true;
            }

            if (hasChanges)
            {
                var updateResult = await userManager.UpdateAsync(adminUser);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Failed to update seeded admin user. {errors}");
                }
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, AdminRoleName))
        {
            var roleResult = await userManager.AddToRoleAsync(adminUser, AdminRoleName);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(", ", roleResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to assign Admin role to seeded admin user. {errors}");
            }
        }
    }

}
