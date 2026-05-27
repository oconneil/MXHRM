using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Authorization;
using MXHRM.Application.Users;
using MXHRM.Application.Users.DTOs;
using MXHRM.Api.Common;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class UsersController(IUserService userService) : BaseApiController
{
    [HttpGet]
    [ProducesResponseType(typeof(List<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await userService.GetAllAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(string id)
    {
        var user = await userService.GetByIdAsync(id);
        return user is null ? NotFoundError("User not found.") : Ok(user);
    }

    [HttpGet("{id}/roles")]
    [ProducesResponseType(typeof(UserRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserRoleResponse>> GetRoles(string id)
    {
        var result = await userService.GetRolesAsync(id);
        return result.Succeeded ? Ok(result.Value) : OperationError(result);
    }

    [HttpPut("{id}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRoles(
    string id,
    UpdateUserRolesRequest request)
    {
        var result = await userService.UpdateRolesAsync(id, request, GetCurrentUserId());
        return result.Succeeded ? NoContent() : OperationError(result);
    }

    [HttpPut("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(string id)
    {
        var result = await userService.ActivateAsync(id);
        return result.Succeeded ? NoContent() : OperationError(result);
    }

    [HttpPut("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
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
