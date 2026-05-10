using MXHRM.Application.Common;
using MXHRM.Application.Users.DTOs;

namespace MXHRM.Application.Users;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();
    Task<UserResponse?> GetByIdAsync(string id);
    Task<OperationResult<UserRoleResponse>> GetRolesAsync(string id);
    Task<OperationResult> UpdateRolesAsync(string id, UpdateUserRolesRequest request, string? currentUserId);
    Task<OperationResult> ActivateAsync(string id);
    Task<OperationResult> DeactivateAsync(string id, string? currentUserId);
}
