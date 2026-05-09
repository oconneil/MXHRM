using MXHRM.Api.DTOs.Roles;

namespace MXHRM.Api.DTOs.Users;

public class UserRoleResponse
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<RoleResponse> Roles { get; set; } = new();
}
