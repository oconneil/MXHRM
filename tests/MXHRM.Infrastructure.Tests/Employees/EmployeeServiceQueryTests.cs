using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MXHRM.Application.Auditing;
using MXHRM.Application.Common;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Application.Employees.DTOs;
using MXHRM.Domain.Employees;
using MXHRM.Infrastructure.Data;
using MXHRM.Infrastructure.Employees;
using Xunit;

namespace MXHRM.Infrastructure.Tests.Employees;

public class EmployeeServiceQueryTests
{
    // สร้าง InMemory DB ใหม่ทุกครั้ง (Guid = ชื่อ db ไม่ซ้ำ → เทสต์ไม่ปนกัน)
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_on_cache_miss_should_query_db_and_write_cache()
    {
        // ---------- Arrange ----------
        await using var db = CreateInMemoryDb();
        db.Employees.AddRange(
            new Employee
            {
                CompanyID = "C001",
                EmployeeID = "E001",
                FirstName = "Ann",
                LastName = "A",
                Email = "ann@x.com",
                IsActive = true
            },
            new Employee
            {
                CompanyID = "C001",
                EmployeeID = "E002",
                FirstName = "Bob",
                LastName = "B",
                Email = "bob@x.com",
                IsActive = true
            });
        await db.SaveChangesAsync();

        var cache = new Mock<ICacheService>();
        // จำลอง cache MISS → GetAsync คืน null
        cache
            .Setup(c => c.GetAsync<PagedResponse<EmployeeResponse>>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PagedResponse<EmployeeResponse>?)null);

        var sut = new EmployeeService(
            db,                                          // ← DB จริง (InMemory)
            NullLogger<EmployeeService>.Instance,
            cache.Object,
            new ConfigurationBuilder().Build(),          // config ว่าง → ใช้ค่า default
            Mock.Of<IAuditLogService>());

        // ---------- Act ----------
        var result = await sut.GetAllAsync(new GetEmployeesRequest { Page = 1, PageSize = 10 });

        // ---------- Assert ----------
        Assert.Equal(2, result.TotalItems);             // query เจอ 2 คน
        Assert.Equal(2, result.Items.Count);

        // พิสูจน์ว่า "หลัง query แล้ว เขียน cache จริง" (สำคัญ! เพราะ cache miss ต้อง set)
        cache.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<PagedResponse<EmployeeResponse>>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}