namespace MXHRM.Application.Auth.DTOs;

public class RegisterRequest
{
    public string CompanyID { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;
}