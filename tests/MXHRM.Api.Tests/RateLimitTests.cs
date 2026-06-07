using System.Net;
using System.Net.Http.Json;

namespace MXHRM.Api.Tests;

public class RateLimitTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public RateLimitTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_should_return_429_after_exceeding_limit()
    {
        var client = _factory.CreateClient();

        // body ว่าง → validation 400 (ไม่แตะ DB, เร็ว) แต่ rate limiter นับทุก request
        var payload = new { };

        HttpResponseMessage response = null!;
        for (var i = 0; i < 6; i++)
        {
            response = await client.PostAsJsonAsync("/api/auth/login", payload);
        }

        // policy = 5 ครั้ง/นาที → ครั้งที่ 6 ต้องโดน rate limit
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }
}
