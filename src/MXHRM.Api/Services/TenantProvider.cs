using System.Security.Claims;
using MXHRM.Application.Common.Interfaces;

namespace MXHRM.Api.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string CompanyClaimType = "company_id";

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    // tenant = company_id claim จาก JWT (ปลอมไม่ได้)
    public string? CompanyId => User?.FindFirst(CompanyClaimType)?.Value;

    // bypass เฉพาะตอนไม่ได้ login (Hangfire job / seeder / design-time)
    // admin ไม่ bypass — ถูก scope ตามบริษัทที่เลือกตอน login เหมือนทุกคน
    public bool BypassTenantFilter =>
        User?.Identity?.IsAuthenticated != true;
}