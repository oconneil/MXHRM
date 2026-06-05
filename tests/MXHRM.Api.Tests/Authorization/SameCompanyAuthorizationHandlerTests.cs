using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MXHRM.Api.Authorization;
using Xunit;

namespace MXHRM.Api.Tests.Authorization;

public class SameCompanyAuthorizationHandlerTests
{
    // helper: สร้าง "ผู้ใช้ปลอม" ใส่ company_id + roles
    private static ClaimsPrincipal CreateUser(string companyId, params string[] roles)
    {
        var claims = new List<Claim> { new("company_id", companyId) };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        var identity = new ClaimsIdentity(claims, authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }

    // helper: รัน handler แล้วคืนผลว่า "ผ่านไหม"
    private static async Task<bool> RunAsync(ClaimsPrincipal user, string resourceCompanyId)
    {
        var requirement = new SameCompanyRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement }, user, resource: resourceCompanyId);

        await new SameCompanyAuthorizationHandler().HandleAsync(context);
        return context.HasSucceeded;
    }

    // ===== ตัวอย่างที่ผมเขียนให้ =====
    [Fact]
    public async Task User_accessing_own_company_should_succeed()
    {
        var user = CreateUser("JCORP");              // user อยู่บริษัท JCORP
        Assert.True(await RunAsync(user, "JCORP"));  // ขอเข้า JCORP → ผ่าน
    }

    [Fact]
    public async Task User_accessing_other_company_should_fail()
    {
        var user = CreateUser("JCORP");              // user อยู่บริษัท JCORP
        Assert.False(await RunAsync(user, "OTHER"));  // ขอเข้า OTHER → ไม่ผ่าน
    }

    [Fact]
    public async Task Admin_role_does_not_bypass_company_check()
    {
        // โมเดลใหม่: admin ถูก scope ตามบริษัทที่ login (claim) — role ไม่ใช่ตั๋วผ่านข้ามบริษัท
        var user = CreateUser("JCORP", "Admin");
        Assert.False(await RunAsync(user, "OTHER"));   // claim=JCORP ขอเข้า OTHER → ไม่ผ่าน
    }
}