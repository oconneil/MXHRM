using MXHRM.Api.DTOs.Permissions;

namespace MXHRM.Api.DTOs.Roles;

public class RolePermissionResponse
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public List<PermissionResponse> Permissions { get; set; } = new();
}
