using MXHRM.Application.Roles.DTOs;

namespace MXHRM.Application.Users.DTOs;

public class UserRoleResponse
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public List<RoleResponse> Roles { get; set; } = new();
}
