using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Authorization;
using MXHRM.Infrastructure.Jobs;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class JobsController : BaseApiController
{
    private readonly IBackgroundJobClient _backgroundJobClient;

    public JobsController(IBackgroundJobClient backgroundJobClient)
    {
        _backgroundJobClient = backgroundJobClient;
    }

    [HttpPost("cleanup-refresh-tokens")]
    public IActionResult CleanupRefreshTokens()
    {
        var jobId = _backgroundJobClient.Enqueue<CleanupExpiredRefreshTokensJob>(
            job => job.ExecuteAsync());

        return Accepted(new
        {
            JobId = jobId,
            Message = "Refresh token cleanup job has been queued."
        });
    }
}