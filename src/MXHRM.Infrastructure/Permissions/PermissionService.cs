using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Permissions;
using MXHRM.Application.Permissions.DTOs;
using MXHRM.Infrastructure.Data;

namespace MXHRM.Infrastructure.Permissions;

public class PermissionService(AppDbContext db) : IPermissionService
{
    public async Task<List<PermissionResponse>> GetAllAsync()
    {
        return await db.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new PermissionResponse
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name
            })
            .ToListAsync();
    }
}
