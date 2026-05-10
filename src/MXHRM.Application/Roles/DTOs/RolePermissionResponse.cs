using MXHRM.Application.Permissions.DTOs;

namespace MXHRM.Application.Roles.DTOs;

public class RolePermissionResponse
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionResponse> Permissions { get; set; } = new();
}
