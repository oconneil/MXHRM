namespace MXHRM.Application.Auth.DTOs;

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
    public string CompanyID { get; set; } = string.Empty;
}