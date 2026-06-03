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

public class AuthServiceRefreshTokenTests
{
    private static AppDbContext CreateInMemoryDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static IConfiguration JwtConfig() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "MXHRM.Api",
                ["Jwt:Audience"] = "MXHRM.Web",
                ["Jwt:SecretKey"] = "test-secret-key-at-least-32-characters-long-1234567890",
                ["Jwt:AccessTokenMinutes"] = "15",
            }).Build();

    // ===== เทสต์ success (rotation) ที่ผมเขียนให้ =====
    [Fact]
    public async Task RefreshTokenAsync_with_valid_token_should_rotate_and_return_new_tokens()
    {
        // Arrange — seed refresh token ที่ยัง active
        await using var db = CreateInMemoryDb();
        db.RefreshTokens.Add(new RefreshToken
        {
            Token = "valid-token",
            UserId = "user-1",
            ExpiresAt = DateTime.UtcNow.AddDays(7),   // ยังไม่หมดอายุ
            CreatedAt = DateTime.UtcNow               // RevokedAt = null → IsActive = true
        });
        await db.SaveChangesAsync();

        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync("user-1"))
            .ReturnsAsync(new ApplicationUser
            { Id = "user-1", UserName = "u", CompanyID = "C001", DisplayName = "U", IsActive = true });
        userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string>());        // ไม่มี role → ไม่แตะ DB permission

        var sut = new AuthService(userManager.Object, JwtConfig(), db, Mock.Of<IUserActivityLogService>());

        // Act
        var result = await sut.RefreshTokenAsync(new RefreshTokenRequest { RefreshToken = "valid-token" });

        // Assert
        Assert.False(string.IsNullOrEmpty(result.AccessToken));     // ออก JWT ใหม่
        Assert.NotEqual("valid-token", result.RefreshToken);        // 🔑 refresh token หมุนเป็นตัวใหม่

        var oldToken = await db.RefreshTokens.FirstAsync(t => t.Token == "valid-token");
        Assert.NotNull(oldToken.RevokedAt);                         // 🔑 ตัวเก่าถูก revoke
        Assert.Equal(2, await db.RefreshTokens.CountAsync());       // เก่า(revoked) + ใหม่ = 2
    }

    [Fact]
    public async Task RefreshTokenAsync_with_unknown_token_should_throw()
    {
        await using var db = CreateInMemoryDb();
        var sut = new AuthService(CreateUserManagerMock().Object, JwtConfig(), db, Mock.Of<IUserActivityLogService>());

        var request = new RefreshTokenRequest { RefreshToken = "unknown-token" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.RefreshTokenAsync(request));
    }

    [Fact]
    public async Task RefreshTokenAsync_with_expired_token_should_throw()
    {
        await using var db = CreateInMemoryDb();
        db.RefreshTokens.Add(new RefreshToken
        {
            Token = "expired-token",
            UserId = "user-1",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),  // หมดอายุแล้ว
            CreatedAt = DateTime.UtcNow.AddDays(-8)   // เก่ามากๆ → RevokedAt = null แต่ IsActive = false
        });
        await db.SaveChangesAsync();

        var sut = new AuthService(CreateUserManagerMock().Object, JwtConfig(), db, Mock.Of<IUserActivityLogService>());

        var request = new RefreshTokenRequest { RefreshToken = "expired-token" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.RefreshTokenAsync(request));
    }
}