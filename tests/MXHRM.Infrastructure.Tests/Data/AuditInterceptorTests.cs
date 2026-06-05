using Microsoft.EntityFrameworkCore;
using Moq;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Domain.Employees;
using MXHRM.Infrastructure.Data;
using Xunit;

namespace MXHRM.Infrastructure.Tests.Data;

public class AuditInterceptorTests
{
    private static AppDbContext CreateDbWithInterceptor(ICurrentUserService user, ITenantProvider tenant)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(new AuditSaveChangesInterceptor(user, tenant))
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Interceptor_stamps_tenant_company_and_audit_on_create()
    {
        // Arrange — user "alice" ในบริษัท JAMORE
        var user = Mock.Of<ICurrentUserService>(u => u.UserName == "alice");
        var tenant = Mock.Of<ITenantProvider>(t =>
            t.CompanyId == "JAMORE" && t.BypassTenantFilter == false);

        await using var db = CreateDbWithInterceptor(user, tenant);

        // สร้าง employee โดย "ไม่ใส่" CompanyID และ CreatedBy
        var emp = new Employee { EmployeeID = "E1", FirstName = "A", Email = "a@x.com" };

        // Act
        db.Employees.Add(emp);
        await db.SaveChangesAsync();

        // Assert — interceptor เติมให้หมด
        Assert.Equal("JAMORE", emp.CompanyID);   // auto-stamp จาก tenant
        Assert.Equal("alice", emp.CreatedBy);    // audit จาก JWT
        Assert.Equal("alice", emp.ModifiedBy);
        Assert.NotEqual(default, emp.CreatedDate);
    }

    [Fact]
    public async Task Interceptor_does_not_overwrite_explicit_company()
    {
        var user = Mock.Of<ICurrentUserService>(u => u.UserName == "bob");
        var tenant = Mock.Of<ITenantProvider>(t =>
            t.CompanyId == "JAMORE" && t.BypassTenantFilter == false);

        await using var db = CreateDbWithInterceptor(user, tenant);

        // ระบุ CompanyID เอง = "JCORP" (เช่น seeder)
        var emp = new Employee { CompanyID = "JCORP", EmployeeID = "E1", FirstName = "B", Email = "b@x.com" };

        db.Employees.Add(emp);
        await db.SaveChangesAsync();

        // auto-fill เฉพาะตอนว่าง → ของที่ระบุเองต้องไม่ถูกทับ
        Assert.Equal("JCORP", emp.CompanyID);
    }
}
