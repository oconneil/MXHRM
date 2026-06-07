using Microsoft.AspNetCore.Identity;
using Moq;
using MXHRM.Infrastructure.Identity;
using MXHRM.Infrastructure.Users;
using Xunit;

namespace MXHRM.Infrastructure.Tests.Users;

public class UserServiceSecurityStampTests
{
    private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task DeactivateAsync_should_bump_security_stamp()
    {
        // Arrange
        var user = new ApplicationUser { Id = "u-1", UserName = "bob", IsActive = true };

        var userManager = CreateUserManagerMock();
        userManager.Setup(m => m.FindByIdAsync("u-1")).ReturnsAsync(user);
        userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        userManager.Setup(m => m.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        // roleManager + db ไม่ถูกใช้ใน DeactivateAsync → null! ปลอดภัย
        var sut = new UserService(userManager.Object, null!, null!);

        // Act — admin deactivate user คนอื่น
        var result = await sut.DeactivateAsync("u-1", currentUserId: "admin");

        // Assert — สำเร็จ + stamp ถูก bump → token เก่าของ user คนนี้ใช้ไม่ได้ทันที
        Assert.True(result.Succeeded);
        userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
    }
}
