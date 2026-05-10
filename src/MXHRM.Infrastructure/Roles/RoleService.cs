using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AppPermissions = MXHRM.Application.Authorization.Permissions;
using MXHRM.Application.Common;
using MXHRM.Application.Permissions.DTOs;
using MXHRM.Application.Roles;
using MXHRM.Application.Roles.DTOs;
using MXHRM.Infrastructure.Authorization;
using MXHRM.Infrastructure.Data;

namespace MXHRM.Infrastructure.Roles;

public class RoleService(RoleManager<IdentityRole> roleManager, AppDbContext db) : IRoleService
{
    private static readonly string[] ProtectedRoles = ["Admin", "HR", "Employee"];

    public async Task<List<RoleResponse>> GetAllAsync()
    {
        return await roleManager.Roles
            .AsNoTracking()
            .Select(x => new RoleResponse
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty
            })
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<RoleResponse?> GetByIdAsync(string id)
    {
        return await roleManager.Roles
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new RoleResponse
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty
            })
            .FirstOrDefaultAsync();
    }

    public async Task<OperationResult<RolePermissionResponse>> GetPermissionsAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return OperationResult<RolePermissionResponse>.Failure(OperationErrorType.NotFound, "Role not found.");
        }

        var permissions = await db.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == id)
            .Select(x => new PermissionResponse
            {
                Id = x.Permission.Id,
                Code = x.Permission.Code,
                Name = x.Permission.Name
            })
            .OrderBy(x => x.Code)
            .ToListAsync();

        return OperationResult<RolePermissionResponse>.Success(new RolePermissionResponse
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            Permissions = permissions
        });
    }

    public async Task<OperationResult> UpdatePermissionsAsync(string id, UpdateRolePermissionsRequest request)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return OperationResult.Failure(OperationErrorType.NotFound, "Role not found.");
        }

        var requestedPermissionIds = request.PermissionIds.Distinct().ToList();
        var roleManagePermissionId = await db.Permissions
            .Where(x => x.Code == AppPermissions.Role.Manage)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var isAdminRole = string.Equals(role.Name, "Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdminRole && roleManagePermissionId != 0 && !requestedPermissionIds.Contains(roleManagePermissionId))
        {
            return OperationResult.Failure(OperationErrorType.Validation, "Admin role must keep role.manage permission.");
        }

        var existingPermissionIds = await db.Permissions
            .Where(x => requestedPermissionIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        if (existingPermissionIds.Count != requestedPermissionIds.Count)
        {
            return OperationResult.Failure(OperationErrorType.Validation, "Some permissions do not exist.");
        }

        var currentRolePermissions = await db.RolePermissions
            .Where(x => x.RoleId == id)
            .ToListAsync();

        db.RolePermissions.RemoveRange(currentRolePermissions);
        db.RolePermissions.AddRange(requestedPermissionIds.Select(permissionId => new RolePermission
        {
            RoleId = id,
            PermissionId = permissionId
        }));

        await db.SaveChangesAsync();
        return OperationResult.Success();
    }

    public async Task<OperationResult<RoleResponse>> CreateAsync(CreateRoleRequest request)
    {
        var role = new IdentityRole(request.Name);
        var result = await roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            return OperationResult<RoleResponse>.Failure(OperationErrorType.Validation, "Failed to create role.", result.Errors);
        }

        return OperationResult<RoleResponse>.Success(new RoleResponse
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty
        });
    }

    public async Task<OperationResult> UpdateAsync(string id, UpdateRoleRequest request)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return OperationResult.Failure(OperationErrorType.NotFound, "Role not found.");
        }

        if (ProtectedRoles.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult.Failure(OperationErrorType.Validation, "This role is protected and cannot be updated.");
        }

        role.Name = request.Name;
        var result = await roleManager.UpdateAsync(role);

        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failure(OperationErrorType.Validation, "Failed to update role.", result.Errors);
    }

    public async Task<OperationResult> DeleteAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return OperationResult.Failure(OperationErrorType.NotFound, "Role not found.");
        }

        if (ProtectedRoles.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult.Failure(OperationErrorType.Validation, "This role is protected and cannot be deleted.");
        }

        var result = await roleManager.DeleteAsync(role);

        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failure(OperationErrorType.Validation, "Failed to delete role.", result.Errors);
    }
}
