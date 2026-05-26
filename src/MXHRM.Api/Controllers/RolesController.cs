using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Api.Common;
using MXHRM.Application.Authorization;
using MXHRM.Application.Roles;
using MXHRM.Application.Roles.DTOs;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class RolesController(IRoleService roleService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<RoleResponse>>> GetAll()
    {
        var roles = await roleService.GetAllAsync();
        return Ok(roles);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetById(string id)
    {
        var role = await roleService.GetByIdAsync(id);
        return role is null ? NotFoundError("Role not found.") : Ok(role);
    }

    [HttpGet("{id}/permissions")]
    [ProducesResponseType(typeof(RolePermissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RolePermissionResponse>> GetPermissions(string id)
    {
        var result = await roleService.GetPermissionsAsync(id);
        return result.Succeeded ? Ok(result.Value) : OperationError(result);
    }

    [HttpPut("{id}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePermissions(
    string id,
    UpdateRolePermissionsRequest request)
    {
        var result = await roleService.UpdatePermissionsAsync(id, request);
        return result.Succeeded ? NoContent() : OperationError(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request)
    {
        var result = await roleService.CreateAsync(request);
        return result.Succeeded
            ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
            : OperationError(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
    string id,
    UpdateRoleRequest request)
    {
        var result = await roleService.UpdateAsync(id, request);
        return result.Succeeded ? NoContent() : OperationError(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await roleService.DeleteAsync(id);
        return result.Succeeded ? NoContent() : OperationError(result);
    }
}
