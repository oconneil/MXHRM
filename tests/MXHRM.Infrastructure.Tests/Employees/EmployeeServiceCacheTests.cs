using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MXHRM.Application.Auditing;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Application.Employees.DTOs;
using MXHRM.Infrastructure.Data;
using MXHRM.Infrastructure.Employees;
using MXHRM.Domain.Employees;
using Xunit;

namespace MXHRM.Infrastructure.Tests.Employees;

public class EmployeeServiceCacheTests
{
    private static AppDbContext CreateInMemoryDb() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task CreateAsync_should_invalidate_employee_cache()
    {
        // ---------- Arrange ----------
        await using var db = CreateInMemoryDb();
        var cache = new Mock<ICacheService>();   // "สายลับ" คอยดูว่า cache ถูกเรียกอะไรบ้าง

        var sut = new EmployeeService(
            db,                                  // InMemory DB จริง
            NullLogger<EmployeeService>.Instance,
            cache.Object,
            new ConfigurationBuilder().Build(),
            Mock.Of<IAuditLogService>());        // audit ปลอม (LogAsync คืน Task ว่างให้เอง)

        var request = new CreateEmployeeRequest
        {
            CompanyID = "C001",
            EmployeeID = "E001",
            FirstName = "Ann",
            LastName = "A",
            Email = "ann@x.com",
            HireDate = new DateTime(2024, 1, 1),
            Salary = 50000m,
            CreatedBy = "admin"
        };

        // ---------- Act ----------
        await sut.CreateAsync(request);

        // ---------- Assert ----------
        // 1) list cache (prefix) ต้องถูกลบ — ไม่งั้นหน้า list จะโชว์ข้อมูลเก่า
        cache.Verify(c => c.RemoveByPrefixAsync("employees:list:", It.IsAny<CancellationToken>()),
            Times.Once);

        // 2) detail cache ต้องถูกลบด้วย
        cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_should_invalidate_employee_cache()
    {
        await using var db = CreateInMemoryDb();

        // Arrange — seed employee ที่จะลบ ให้มีอยู่จริงก่อน
        db.Employees.Add(new Employee
        {
            CompanyID = "C001",
            EmployeeID = "E001",
            FirstName = "Ann",
            LastName = "A",
            Email = "ann@x.com",
            IsActive = true
        });
        await db.SaveChangesAsync();

        var cache = new Mock<ICacheService>();

        var sut = new EmployeeService(
            db,
            NullLogger<EmployeeService>.Instance,
            cache.Object,
            new ConfigurationBuilder().Build(),
            Mock.Of<IAuditLogService>());

        // Act
        await sut.DeleteAsync("C001", "E001");

        // Assert — ลบ cache ทั้ง list + detail
        cache.Verify(c => c.RemoveByPrefixAsync("employees:list:", It.IsAny<CancellationToken>()),
            Times.Once);
        cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}