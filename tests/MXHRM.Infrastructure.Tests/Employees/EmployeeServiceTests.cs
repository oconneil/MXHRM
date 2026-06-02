using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MXHRM.Application.Common;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Application.Employees.DTOs;
using MXHRM.Infrastructure.Employees;
using Xunit;

namespace MXHRM.Infrastructure.Tests.Employees;

public class EmployeeServiceTests
{
    [Fact]
    public async Task GetAllAsync_should_return_cached_result_without_touching_database()
    {
        // ---------- Arrange ----------
        // 1) สร้าง "สตันต์แมน" ของ cache
        var cache = new Mock<ICacheService>();

        // 2) เตรียมข้อมูลที่จะให้ cache "แกล้งตอบ" ว่าเจอ (cache hit)
        var cached = new PagedResponse<EmployeeResponse>
        {
            Items = new List<EmployeeResponse>(),
            Page = 1,
            PageSize = 10,
            TotalItems = 0,
            TotalPages = 0
        };

        // 3) กำกับบท: "ถ้ามีคนเรียก GetAsync ด้วย key อะไรก็ตาม → ตอบ cached"
        cache
            .Setup(c => c.GetAsync<PagedResponse<EmployeeResponse>>(
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        // 4) ประกอบ service โดยใส่ของจริงเฉพาะที่ cache-hit ใช้
        var sut = new EmployeeService(
            db: null!,                                   // cache-hit ไม่แตะ DB → null ปลอดภัย
            logger: NullLogger<EmployeeService>.Instance, // logger เปล่าๆ ไม่ทำอะไร
            cache: cache.Object,                         // เอาตัวปลอมเข้าฉาก
            configuration: null!,                        // ไม่ถูกใช้ใน path นี้
            auditLogService: null!);                     // อ่านข้อมูลไม่ต้อง audit

        // ---------- Act ----------
        var result = await sut.GetAllAsync(new GetEmployeesRequest());

        // ---------- Assert ----------
        Assert.Same(cached, result);                     // ต้องได้ object เดิมจาก cache เป๊ะ
        cache.Verify(c => c.GetAsync<PagedResponse<EmployeeResponse>>(
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once); // ถูกเรียก 1 ครั้ง
    }
}