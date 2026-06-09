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
using MXHRM.Infrastructure.Tests.Common;

namespace MXHRM.Infrastructure.Tests.Employees;

public class EmployeeConcurrencyTests : IClassFixture<SqlServerFixture>
{
    private readonly SqlServerFixture _fixture;

    public EmployeeConcurrencyTests(SqlServerFixture fixture) => _fixture = fixture;

    // สร้าง service ผูกกับ context ที่ส่งเข้ามา (deps อื่น mock ทิ้งหมด)
    private static EmployeeService CreateService(AppDbContext db)
    {
        var cache = new Mock<ICacheService>();   // loose → GetAsync คืน null = cache miss → อ่าน DB จริง
        var audit = new Mock<IAuditLogService>();
        var tenant = new Mock<ITenantProvider>();
        tenant.SetupGet(t => t.BypassTenantFilter).Returns(true);

        return new EmployeeService(
            db,
            NullLogger<EmployeeService>.Instance,
            cache.Object,
            new ConfigurationBuilder().Build(),
            audit.Object,
            tenant.Object);
    }

    [Fact]
    public async Task UpdateAsync_with_stale_RowVersion_should_throw_ConcurrencyConflict()
    {
        const string company = "JAMORE";
        const string empId = "E-CONCURRENCY";

        // Arrange — seed พนักงาน 1 คน (ตั้ง CompanyID เองเพราะไม่ได้ใช้ interceptor)
        await using (var seed = _fixture.CreateContext())
        {
            seed.Employees.Add(new Employee
            {
                CompanyID = company,
                EmployeeID = empId,
                FirstName = "Ann",
                LastName = "Lee",
                Email = "ann@jamore.co",
                HireDate = new DateTime(2020, 1, 1),
                Salary = 30000,
                IsActive = true
            });
            await seed.SaveChangesAsync();
        }

        // อ่าน RowVersion ปัจจุบัน = จำลอง "ทั้ง 2 user เปิดฟอร์มพร้อมกัน ได้ version เดียวกัน"
        byte[] staleRowVersion;
        await using (var read = _fixture.CreateContext())
        {
            var current = await CreateService(read).GetByIdAsync(company, empId);
            staleRowVersion = current!.RowVersion;
        }

        // Act 1 — user A กด save ก่อน → สำเร็จ → RowVersion ใน DB ขยับ
        await using (var ctxA = _fixture.CreateContext())
        {
            await CreateService(ctxA).UpdateAsync(company, empId, new UpdateEmployeeRequest
            {
                FirstName = "Ann",
                LastName = "Lee",
                Email = "ann@jamore.co",
                HireDate = new DateTime(2020, 1, 1),
                Salary = 35000,            // A ขึ้นเงินเดือน
                IsActive = true,
                RowVersion = staleRowVersion
            });
        }

        // Act 2 + Assert — user B กด save ทีหลัง โดยถือ RowVersion เก่า → ต้องโดน conflict (กัน lost update)
        await using (var ctxB = _fixture.CreateContext())
        {
            var serviceB = CreateService(ctxB);

            await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
                serviceB.UpdateAsync(company, empId, new UpdateEmployeeRequest
                {
                    FirstName = "Annie",
                    LastName = "Lee",
                    Email = "ann@jamore.co",
                    HireDate = new DateTime(2020, 1, 1),
                    Salary = 31000,        // B แก้บนข้อมูลเก่า → ต้องถูกบล็อก ไม่ทับงาน A
                    IsActive = true,
                    RowVersion = staleRowVersion
                }));
        }
    }
}