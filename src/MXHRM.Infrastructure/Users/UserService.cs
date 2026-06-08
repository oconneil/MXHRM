using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AppPermissions = MXHRM.Application.Authorization.Permissions;
using MXHRM.Application.Common;
using MXHRM.Application.Roles.DTOs;
using MXHRM.Application.Users;
using MXHRM.Application.Users.DTOs;
using MXHRM.Infrastructure.Data;
using MXHRM.Infrastructure.Identity;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Infrastructure.Auth;

namespace MXHRM.Infrastructure.Users;

public class UserService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    AppDbContext db,
    ICacheService cache) : IUserService
{
    public async Task<List<UserResponse>> GetAllAsync()
    {
        return await userManager.Users
            .AsNoTracking()
            .OrderBy(x => x.UserName)
            .Select(x => new UserResponse
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                CompanyID = x.CompanyID,
                DisplayName = x.DisplayName,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<UserResponse?> GetByIdAsync(string id)
    {
        return await userManager.Users
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new UserResponse
            {
                Id = x.Id,
                UserName = x.UserName ?? string.Empty,
                CompanyID = x.CompanyID,
                DisplayName = x.DisplayName,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<OperationResult<UserRoleResponse>> GetRolesAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return OperationResult<UserRoleResponse>.Failure(OperationErrorType.NotFound, "User not found.");
        }

        var userRoleNames = await userManager.GetRolesAsync(user);
        var roles = await roleManager.Roles
            .AsNoTracking()
            .Where(x => userRoleNames.Contains(x.Name!))
            .Select(x => new RoleResponse
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty
            })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return OperationResult<UserRoleResponse>.Success(new UserRoleResponse
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Roles = roles
        });
    }

    public async Task<OperationResult> UpdateRolesAsync(
        string id,
        UpdateUserRolesRequest request,
        string? currentUserId)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return OperationResult.Failure(OperationErrorType.NotFound, "User not found.");
        }

        var requestedRoleIds = request.RoleIds.Distinct().ToList();
        var requestedRoles = await roleManager.Roles
            .Where(x => requestedRoleIds.Contains(x.Id))
            .ToListAsync();

        if (requestedRoles.Count != requestedRoleIds.Count)
        {
            return OperationResult.Failure(OperationErrorType.Validation, "Some roles do not exist.");
        }

        var roleManagePermissionId = await db.Permissions
            .Where(x => x.Code == AppPermissions.Role.Manage)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var roleIdsWithRoleManage = await db.RolePermissions
            .Where(x => x.PermissionId == roleManagePermissionId)
            .Select(x => x.RoleId)
            .ToListAsync();

        var isUpdatingSelf = string.Equals(currentUserId, user.Id, StringComparison.OrdinalIgnoreCase);
        var willKeepRoleManage = requestedRoleIds.Any(roleIdsWithRoleManage.Contains);

        if (isUpdatingSelf && !willKeepRoleManage)
        {
            return OperationResult.Failure(
                OperationErrorType.Validation,
                "You must keep at least one role with role.manage permission for yourself.");
        }

        var currentRoleNames = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(user, currentRoleNames);

        if (!removeResult.Succeeded)
        {
            return OperationResult.Failure(OperationErrorType.Validation, "Failed to update user roles.", removeResult.Errors);
        }

        var newRoleNames = requestedRoles.Select(x => x.Name!).ToList();
        var addResult = await userManager.AddToRolesAsync(user, newRoleNames);

        if (!addResult.Succeeded)
        {
            return OperationResult.Failure(OperationErrorType.Validation, "Failed to update user roles.", addResult.Errors);
        }

        // เปลี่ยน role → bump stamp → token เก่าของ user คนนี้ใช้ไม่ได้ทันที
        await userManager.UpdateSecurityStampAsync(user);
        await cache.RemoveAsync(AuthCacheKeys.UserSecurity(user.Id));

        return OperationResult.Success();
    }

    public async Task<OperationResult> ActivateAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return OperationResult.Failure(OperationErrorType.NotFound, "User not found.");
        }

        user.IsActive = true;
        var result = await userManager.UpdateAsync(user);
        await cache.RemoveAsync(AuthCacheKeys.UserSecurity(user.Id));

        return result.Succeeded
            ? OperationResult.Success()
            : OperationResult.Failure(OperationErrorType.Validation, "Failed to activate user.", result.Errors);
    }

    public async Task<OperationResult> DeactivateAsync(string id, string? currentUserId)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return OperationResult.Failure(OperationErrorType.NotFound, "User not found.");
        }

        var isDeactivatingSelf = string.Equals(currentUserId, user.Id, StringComparison.OrdinalIgnoreCase);
        if (isDeactivatingSelf)
        {
            return OperationResult.Failure(OperationErrorType.Validation, "You cannot deactivate yourself.");
        }

        user.IsActive = false;
        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return OperationResult.Failure(OperationErrorType.Validation, "Failed to deactivate user.", result.Errors);
        }

        await userManager.UpdateSecurityStampAsync(user);
        await cache.RemoveAsync(AuthCacheKeys.UserSecurity(user.Id));

        return OperationResult.Success();
    }
}
