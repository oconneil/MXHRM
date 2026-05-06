namespace MXHRM.Api.Models;

public class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;   // เช่น employee.read
    public string Name { get; set; } = string.Empty;   // เช่น Employee Read

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
