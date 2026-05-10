namespace MXHRM.Application.Roles.DTOs;

public class UpdateRolePermissionsRequest
{
    public List<int> PermissionIds { get; set; } = new();
}
