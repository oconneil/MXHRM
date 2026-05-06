using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Authorization;
using MXHRM.Api.DTOs.Roles;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.RoleManage)]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
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
            return NotFound();

        return Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request)
    {
        var role = new IdentityRole(request.Name);

        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

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
            return NotFound();

        var protectedRoles = new[] { "Admin", "HR", "Employee" };

        if (protectedRoles.Contains(role.Name))
        {
            return BadRequest(new { message = "This role is protected and cannot be updated." });
        }

        role.Name = request.Name;

        var result = await _roleManager.UpdateAsync(role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        if (role is null)
            return NotFound();

        var protectedRoles = new[] { "Admin", "HR", "Employee" };

        if (protectedRoles.Contains(role.Name))
        {
            return BadRequest(new { message = "This role is protected and cannot be deleted." });
        }

        var result = await _roleManager.DeleteAsync(role);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return NoContent();
    }
}
