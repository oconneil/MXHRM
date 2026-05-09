using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Authorization;
using MXHRM.Api.DTOs.Roles;
using MXHRM.Api.DTOs.Users;
using MXHRM.Api.Models;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using MXHRM.Api.Data;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _db;

    public UsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext db)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _db = db;
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<UserRoleResponse>> GetRoles(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        var userRoleNames = await _userManager.GetRolesAsync(user);

        var roles = await _roleManager.Roles
            .AsNoTracking()
            .Where(x => userRoleNames.Contains(x.Name!))
            .Select(x => new RoleResponse
            {
                Id = x.Id,
                Name = x.Name ?? string.Empty
            })
            .OrderBy(x => x.Name)
            .ToListAsync();

        return Ok(new UserRoleResponse
        {
            UserId = user.Id,
            UserName = user.UserName ?? string.Empty,
            Roles = roles
        });
    }

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateRoles(
        string id,
        UpdateUserRolesRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var requestedRoleIds = request.RoleIds
            .Distinct()
            .ToList();

        var requestedRoles = await _roleManager.Roles
            .Where(x => requestedRoleIds.Contains(x.Id))
            .ToListAsync();

        if (requestedRoles.Count != requestedRoleIds.Count)
        {
            return BadRequest(new
            {
                message = "Some roles do not exist."
            });
        }

        var roleManagePermissionId = await _db.Permissions
            .Where(x => x.Code == Permissions.Role.Manage)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        var roleIdsWithRoleManage = await _db.RolePermissions
            .Where(x => x.PermissionId == roleManagePermissionId)
            .Select(x => x.RoleId)
            .ToListAsync();

        var isUpdatingSelf = string.Equals(
            currentUserId,
            user.Id,
            StringComparison.OrdinalIgnoreCase);

        var willKeepRoleManage = requestedRoleIds
            .Any(roleIdsWithRoleManage.Contains);

        if (isUpdatingSelf && !willKeepRoleManage)
        {
            return BadRequest(new
            {
                message = "You must keep at least one role with role.manage permission for yourself."
            });
        }


        var currentRoleNames = await _userManager.GetRolesAsync(user);

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoleNames);

        if (!removeResult.Succeeded)
        {
            return BadRequest(removeResult.Errors);
        }

        var newRoleNames = requestedRoles
            .Select(x => x.Name!)
            .ToList();

        var addResult = await _userManager.AddToRolesAsync(user, newRoleNames);

        if (!addResult.Succeeded)
        {
            return BadRequest(addResult.Errors);
        }

        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await _userManager.Users
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

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var user = await _userManager.Users
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

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        user.IsActive = true;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var isDeactivatingSelf = string.Equals(
            currentUserId,
            user.Id,
            StringComparison.OrdinalIgnoreCase);

        if (isDeactivatingSelf)
        {
            return BadRequest(new
            {
                message = "You cannot deactivate yourself."
            });
        }

        user.IsActive = false;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return NoContent();
    }

}
