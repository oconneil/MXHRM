using Microsoft.AspNetCore.Identity;

namespace MXHRM.Api.Models;

public class ApplicationUser : IdentityUser
{
    public string CompanyID { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}