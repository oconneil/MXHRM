using Microsoft.EntityFrameworkCore;
using Moq;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Domain.Employees;
using MXHRM.Infrastructure.Data;
using Xunit;

namespace MXHRM.Infrastructure.Tests.Data;

public class TenantQueryFilterTests
{
    // สร้าง DB + seed พนักงาน 2 บริษัท (C001, C002) ภายใต้ tenant ที่กำหนด
    private static AppDbContext CreateSeededDb(ITenantProvider? tenant)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options, tenant);
        db.Employees.AddRange(
            new Employee { CompanyID = "C001", EmployeeID = "E1", FirstName = "Ann", Email = "ann@x.com" },
            new Employee { CompanyID = "C002", EmployeeID = "E2", FirstName = "Bob", Email = "bob@x.com" });
        db.SaveChanges();
        return db;
    }

    [Fact]
    public async Task Query_should_return_only_current_tenant_company()
    {
        // tenant = C001, ไม่ bypass
        var tenant = Mock.Of<ITenantProvider>(t =>
            t.CompanyId == "C001" && t.BypassTenantFilter == false);

        await using var db = CreateSeededDb(tenant);

        var employees = await db.Employees.ToListAsync();

        // ถึงในฐานข้อมูลจะมี 2 บริษัท แต่ query เห็นเฉพาะ C001
        Assert.Single(employees);
        Assert.Equal("C001", employees[0].CompanyID);
    }

    [Fact]
    public async Task Bypass_should_return_all_companies()
    {
        // tenant = null (เช่น job/seeder) → bypass = เห็นทุกบริษัท
        await using var db = CreateSeededDb(tenant: null);

        var employees = await db.Employees.ToListAsync();

        Assert.Equal(2, employees.Count);
    }
}
