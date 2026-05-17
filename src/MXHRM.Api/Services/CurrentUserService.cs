using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MXHRM.Application.Common.Interfaces;

namespace MXHRM.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId =>
        User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

    public string? UserName =>
        User?.FindFirstValue(JwtRegisteredClaimNames.UniqueName)
        ?? User?.Identity?.Name;

    public string? TraceId =>
        _httpContextAccessor.HttpContext?.TraceIdentifier;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;
}