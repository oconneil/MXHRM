using MXHRM.Application.Permissions.DTOs;

namespace MXHRM.Application.Permissions;

public interface IPermissionService
{
    Task<List<PermissionResponse>> GetAllAsync();
}
