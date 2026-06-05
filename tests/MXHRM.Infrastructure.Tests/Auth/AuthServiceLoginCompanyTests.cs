using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MXHRM.Application.Auditing;
using MXHRM.Application.Auth.DTOs;
using MXHRM.Infrastructure.Auth;
using MXHRM.Infrastructure.Data;
using MXHRM.Infrastructure.Identity;
using Xunit;

namespace MXHRM.Infrastructure.Tests.Auth;

public class AuthServiceLoginCompanyTests
{
    private static AppDbContext CreateInMemoryDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static IConfiguration JwtConfig() =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "MXHRM.Api",
            ["Jwt:Audience"] = "MXHRM.Web",
            ["Jwt:SecretKey"] = "test-secret-key-at-least-32-characters-long-1234567890",
            ["Jwt:AccessTokenMinutes"] = "15",
        }).Build();

    // ===== ตัวอย่างที่ผมเขียนให้: admin login เข้าบริษัทไหนก็ได้ =====
    [Fact]
    public async Task Admin_can_login_to_any_company()
    {
        await using var db = CreateInMemoryDb();
        var user = new ApplicationUser
        { Id = "u-1", UserName = "admin", CompanyID = "JCORP", DisplayName = "Admin", IsActive = true };

        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
        userManager.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(true);          // ← เป็น admin
        userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string>());

        var sut = new AuthService(userManager.Object, JwtConfig(), db, Mock.Of<IUserActivityLogService>());

        // admin อยู่ JCORP แต่ขอเข้าบริษัท OTHER
        var result = await sut.LoginAsync(
            new LoginRequest { UserName = "admin", Password = "x", CompanyID = "OTHER" });

        Assert.Equal("OTHER", result.CompanyID);   // ← admin ได้บริษัทที่เลือก
    }

    // non-admin ห้าม login เข้าบริษัทอื่นที่ไม่ใช่ของตัวเอง (security)
    [Fact]
    public async Task Non_admin_with_wrong_company_should_throw()
    {
        await using var db = CreateInMemoryDb();
        var user = new ApplicationUser
        { Id = "u-2", UserName = "bob", CompanyID = "JCORP", DisplayName = "Bob", IsActive = true };

        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.FindByNameAsync(It.IsAny<string>())).ReturnsAsync(user);
        userManager.Setup(m => m.CheckPasswordAsync(user, It.IsAny<string>())).ReturnsAsync(true);
        userManager.Setup(m => m.IsInRoleAsync(user, "Admin")).ReturnsAsync(false);   // ไม่ใช่ admin

        var sut = new AuthService(userManager.Object, JwtConfig(), db, Mock.Of<IUserActivityLogService>());

        // bob อยู่ JCORP แต่พยายาม login เข้า OTHER → ต้องถูกปฏิเสธ
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.LoginAsync(new LoginRequest { UserName = "bob", Password = "x", CompanyID = "OTHER" }));
    }
}