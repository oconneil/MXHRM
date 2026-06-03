using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MXHRM.Api.Tests;

// WebApplicationFactory boots the real Program.cs in-memory and gives us an HttpClient.
//
// IMPORTANT (config timing): AddInfrastructure() reads configuration DURING service
// registration — i.e. BEFORE builder.Build(). A factory's ConfigureAppConfiguration only
// applies at Build() time, which is too late. So we feed the values as environment
// variables, which WebApplication.CreateBuilder() reads immediately at startup.
// ("__" in an env var name maps to ":" in a config key.)
public class ApiFactory : WebApplicationFactory<Program>
{
    public ApiFactory()
    {
        // dummy values just to satisfy startup config checks (never actually connected)
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", "Server=test;Database=test;");
        Environment.SetEnvironmentVariable("Redis__ConnectionString", "localhost:6379");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "MXHRM.Api");
        Environment.SetEnvironmentVariable("Jwt__Audience", "MXHRM.Web");
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "test-secret-key-at-least-32-characters-long-1234567890");
        Environment.SetEnvironmentVariable("Hangfire__UseServer", "false"); // ไม่ start Hangfire worker → ไม่ต่อ SQL
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Environment "Testing" → Program.cs skips DB seeding + recurring jobs
        builder.UseEnvironment("Testing");
    }
}
