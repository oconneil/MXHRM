using Microsoft.AspNetCore.Authorization;

namespace MXHRM.Api.Authorization;

// resource = companyId (string) ที่ถูกร้องขอ
public sealed class SameCompanyAuthorizationHandler
    : AuthorizationHandler<SameCompanyRequirement, string>
{
    private const string CompanyClaimType = "company_id";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameCompanyRequirement requirement,
        string resourceCompanyId)
    {
        // ทุกคน (รวม admin) ถูก scope ตาม company_id claim ที่ login เข้ามา
        // admin อยากเข้าบริษัทอื่น = login ใหม่ด้วย CompanyID นั้น (สอดคล้องกับ tenant query filter)
        var userCompanyId = context.User.FindFirst(CompanyClaimType)?.Value;

        if (!string.IsNullOrEmpty(userCompanyId) &&
            string.Equals(userCompanyId, resourceCompanyId, StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
