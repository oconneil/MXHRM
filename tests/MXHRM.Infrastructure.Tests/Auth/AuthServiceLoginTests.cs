using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using MXHRM.Application.Auditing;
using MXHRM.Application.Auth.DTOs;
using MXHRM.Infrastructure.Auth;
using MXHRM.Infrastructure.Identity;
using MXHRM.Application.Auditing.DTOs;
using Xunit;

namespace MXHRM.Infrastructure.Tests.Auth;

public class AuthServiceLoginTests
{
    // 🦖 helper สร้าง mock ของ UserManager (constructor มี 9 args — ป้อน store ปลอม + null ที่เหลือ)
    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    // helper ประกอบ AuthService (failure path ไม่แตะ _db → null! ปลอดภัย)
    private static AuthService CreateSut(
        Mock<UserManager<ApplicationUser>> userManager,
        Mock<IUserActivityLogService> activityLog)
        => new(
            userManager.Object,
            new ConfigurationBuilder().Build(),
            null!,
            activityLog.Object);

    // ===== ตัวอย่างที่ผมเขียนให้ =====
    [Fact]
    public async Task LoginAsync_with_unknown_username_should_throw_and_log_failure()
    {
        // Arrange
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);          // ไม่เจอ user

        var activityLog = new Mock<IUserActivityLogService>();
        var sut = CreateSut(userManager, activityLog);

        var request = new LoginRequest { UserName = "ghost", Password = "x" };

        // Act + Assert — ต้องโยน UnauthorizedAccessException
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.LoginAsync(request));

        // และต้องบันทึก activity log (security audit) 1 ครั้ง
        activityLog.Verify(
            a => a.LogAsync(It.IsAny<CreateUserActivityLogRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_with_inactive_user_should_throw()
    {
        // Arrange
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { UserName = "inactive_user", IsActive = false });  
        
        var activityLog = new Mock<IUserActivityLogService>();
        var sut = CreateSut(userManager, activityLog);

        var request = new LoginRequest { UserName = "inactive_user", Password = "x" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.LoginAsync(request));

        activityLog.Verify(
            a => a.LogAsync(It.IsAny<CreateUserActivityLogRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_with_wrong_password_should_throw()
    {
        // Arrange
        var userManager = CreateUserManagerMock();
        userManager
            .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { UserName = "user1", IsActive = true });  // เจอ user แต่ password ผิด
        userManager
            .Setup(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);  // password ไม่ถูกต้อง

        var activityLog = new Mock<IUserActivityLogService>();
        var sut = CreateSut(userManager, activityLog);

        var request = new LoginRequest { UserName = "user1", Password = "wrong_password" };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => sut.LoginAsync(request));

        activityLog.Verify(
            a => a.LogAsync(It.IsAny<CreateUserActivityLogRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}