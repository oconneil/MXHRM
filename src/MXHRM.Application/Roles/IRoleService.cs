using MXHRM.Application.Common;
using MXHRM.Application.Roles.DTOs;

namespace MXHRM.Application.Roles;

public interface IRoleService
{
    Task<List<RoleResponse>> GetAllAsync();
    Task<RoleResponse?> GetByIdAsync(string id);
    Task<OperationResult<RolePermissionResponse>> GetPermissionsAsync(string id);
    Task<OperationResult> UpdatePermissionsAsync(string id, UpdateRolePermissionsRequest request);
    Task<OperationResult<RoleResponse>> CreateAsync(CreateRoleRequest request);
    Task<OperationResult> UpdateAsync(string id, UpdateRoleRequest request);
    Task<OperationResult> DeleteAsync(string id);
}
