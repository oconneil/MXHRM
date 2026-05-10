using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Authorization;
using MXHRM.Application.Permissions;
using MXHRM.Application.Permissions.DTOs;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class PermissionsController(IPermissionService permissionService) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<List<PermissionResponse>>> GetAll()
    {
        var permissions = await permissionService.GetAllAsync();
        return Ok(permissions);
    }
}
