namespace MXHRM.Application.Users.DTOs;

public class UpdateUserRolesRequest
{
    public List<string> RoleIds { get; set; } = new();
}
