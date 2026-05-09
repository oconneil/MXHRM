namespace MXHRM.Api.DTOs.Users;

public class UpdateUserRolesRequest
{
    public List<string> RoleIds { get; set; } = new();
}
