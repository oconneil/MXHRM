using System.Net;

namespace MXHRM.Api.Tests;

// IClassFixture<ApiFactory> = boot app "ครั้งเดียว" แล้ว share ให้ทุกเทสต์ในคลาส (เร็ว)
public class HealthEndpointTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public HealthEndpointTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_endpoint_should_return_200()
    {
        // Arrange — สร้าง HttpClient ที่ยิงเข้า app จำลอง
        var client = _factory.CreateClient();

        // Act — ยิง HTTP จริงเข้า pipeline (Kestrel → routing → endpoint)
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Protected_endpoint_without_token_should_return_401()
    {
        var client = _factory.CreateClient();

        // /api/employees มี [Authorize] → ไม่มี token ต้องโดน 401
        var response = await client.GetAsync("/api/employees");

        // ทดสอบว่า auth middleware + custom OnChallenge ทำงาน
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}