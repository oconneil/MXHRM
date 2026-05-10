using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Authorization;
using MXHRM.Application.Users;
using MXHRM.Application.Users.DTOs;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class UsersController(IUserService userService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var user = await userService.GetByIdAsync(id);
        return user is null ? NotFoundError("User not found.") : Ok(user);
    }

    [HttpGet("{id}/roles")]
    public async Task<ActionResult<UserRoleResponse>> GetRoles(string id)
    {
        var result = await userService.GetRolesAsync(id);
        return result.Succeeded ? Ok(result.Value) : OperationError(result);
    }

    [HttpPut("{id}/roles")]
    public async Task<IActionResult> UpdateRoles(string id, UpdateUserRolesRequest request)
    {
        var result = await userService.UpdateRolesAsync(id, request, GetCurrentUserId());
        return result.Succeeded ? NoContent() : OperationError(result);
    }

    [HttpPut("{id}/activate")]
    public async Task<IActionResult> Activate(string id)
    {
        var result = await userService.ActivateAsync(id);
        return result.Succeeded ? NoContent() : OperationError(result);
    }

    [HttpPut("{id}/deactivate")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var result = await userService.DeactivateAsync(id, GetCurrentUserId());
        return result.Succeeded ? NoContent() : OperationError(result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
    }
}
