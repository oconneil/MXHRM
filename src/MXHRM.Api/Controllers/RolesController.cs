using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Authorization;
using MXHRM.Api.DTOs.Roles;
using MXHRM.Api.Data;
using MXHRM.Api.DTOs.Permissions;
using MXHRM.Api.Models;


namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class RolesController : BaseApiController
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _db;
    public RolesController(
        RoleManager<IdentityRole> roleManager,
        AppDbContext db)
    {
        _roleManager = roleManager;
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleResponse>>> GetAll()
    {
        var roles = await _roleManager.Roles
            .Select(x => new RoleResponse
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty
            })
            .ToListAsync();

        return Ok(roles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoleResponse>> GetById(string id)
    {
        var role = await _roleManager.Roles
            .Where(x => x.Id == id)
            .Select(x => new RoleResponse
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty
            })
            .FirstOrDefaultAsync();

        if (role is null)
            return NotFoundError("Role not found.");

        return Ok(role);
    }

    [HttpGet("{id}/permissions")]
    public async Task<ActionResult<RolePermissionResponse>> GetPermissions(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
        {
            return NotFoundError("Role not found.");
        }

        var permissions = await _db.RolePermissions
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

        return Ok(new RolePermissionResponse
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            Permissions = permissions
        });
    }

    [HttpPut("{id}/permissions")]
    public async Task<IActionResult> UpdatePermissions(
    string id,
    UpdateRolePermissionsRequest request)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
        {
            return NotFoundError("Role not found.");
        }

        var isAdminRole = string.Equals(
            role.Name,
            "Admin",
            StringComparison.OrdinalIgnoreCase);

        var requestedPermissionIds = request.PermissionIds
            .Distinct()
            .ToList();

        var roleManagePermissionId = await _db.Permissions
            .Where(x => x.Code == Permissions.Role.Manage)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (isAdminRole && roleManagePermissionId != 0 && !requestedPermissionIds.Contains(roleManagePermissionId))
        {
            return BadRequestError("Admin role must keep role.manage permission.");
        }

        var existingPermissionIds = await _db.Permissions
            .Where(x => requestedPermissionIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();

        if (existingPermissionIds.Count != requestedPermissionIds.Count)
        {
            return BadRequestError("Some permissions do not exist.");
        }

        var currentRolePermissions = await _db.RolePermissions
            .Where(x => x.RoleId == id)
            .ToListAsync();

        _db.RolePermissions.RemoveRange(currentRolePermissions);

        var newRolePermissions = requestedPermissionIds.Select(permissionId => new RolePermission
        {
            RoleId = id,
            PermissionId = permissionId
        });

        _db.RolePermissions.AddRange(newRolePermissions);

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request)
    {
        var role = new IdentityRole(request.Name);

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            return BadRequestError("Failed to create role.", result.Errors);

        var response = new RoleResponse
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty
        };

        return CreatedAtAction(nameof(GetById), new { id = role.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, UpdateRoleRequest request)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
            return NotFoundError("Role not found.");

        var protectedRoles = new[] { "Admin", "HR", "Employee" };

        if (protectedRoles.Contains(role.Name))
        {
            return BadRequestError("This role is protected and cannot be updated.");
        }

        role.Name = request.Name;

        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
            return BadRequestError("Failed to update role.", result.Errors);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
            return NotFoundError("Role not found.");

        var protectedRoles = new[] { "Admin", "HR", "Employee" };

        if (protectedRoles.Contains(role.Name))
        {
            return BadRequestError("This role is protected and cannot be deleted.");
        }

        var result = await _roleManager.DeleteAsync(role);

        if (!result.Succeeded)
            return BadRequestError("Failed to delete role.", result.Errors);

        return NoContent();
    }
}
