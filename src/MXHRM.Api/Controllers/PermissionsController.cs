using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Authorization;
using MXHRM.Api.Data;
using MXHRM.Api.DTOs.Permissions;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class PermissionsController : BaseApiController
{
    private readonly AppDbContext _db;

    public PermissionsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<PermissionResponse>>> GetAll()
    {
        var permissions = await _db.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new PermissionResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name
            })
            .ToListAsync();

        return Ok(permissions);
    }
}
