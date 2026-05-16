using Microsoft.Extensions.Logging;

namespace MXHRM.Infrastructure.Jobs;

public class SystemHealthJob
{
    private readonly ILogger<SystemHealthJob> _logger;

    public SystemHealthJob(ILogger<SystemHealthJob> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync()
    {
        _logger.LogInformation(
            "System health job executed at {ExecutedAtUtc}",
            DateTime.UtcNow);

        return Task.CompletedTask;
    }
}