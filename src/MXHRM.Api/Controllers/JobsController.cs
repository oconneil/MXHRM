using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Authorization;
using MXHRM.Infrastructure.Jobs;
using System.Text.Json;
using MXHRM.Application.Auditing;
using MXHRM.Application.Auditing.DTOs;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Role.Manage)]
public class JobsController : BaseApiController
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IUserActivityLogService _userActivityLogService;

    public JobsController(
        IBackgroundJobClient backgroundJobClient,
        IUserActivityLogService userActivityLogService)
    {
        _backgroundJobClient = backgroundJobClient;
        _userActivityLogService = userActivityLogService;
    }

    private static string SerializeActivityMetadata(object value)
    {
        return JsonSerializer.Serialize(value);
    }

    [HttpPost("cleanup-refresh-tokens")]
    public async Task<IActionResult> CleanupRefreshTokens()
    {
        var jobId = _backgroundJobClient.Enqueue<CleanupExpiredRefreshTokensJob>(
            job => job.ExecuteAsync());

        await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
        {
            ActivityType = "ManualJobTrigger",
            Description = "Admin manually triggered refresh token cleanup job.",
            Metadata = SerializeActivityMetadata(new
            {
                JobId = jobId,
                JobName = "cleanup-expired-refresh-tokens"
            })
        });

        return Accepted(new
        {
            JobId = jobId,
            Message = "Refresh token cleanup job has been queued."
        });
    }

    [HttpPost("employee-summary-report")]
    public async Task<IActionResult> GenerateEmployeeSummaryReport()
    {
        var jobId = _backgroundJobClient.Enqueue<EmployeeReportJob>(
            job => job.ExecuteAsync());

        await _userActivityLogService.LogAsync(new CreateUserActivityLogRequest
        {
            ActivityType = "ManualJobTrigger",
            Description = "Admin manually triggered employee summary report job.",
            Metadata = SerializeActivityMetadata(new
            {
                JobId = jobId,
                JobName = "employee-summary-report"
            })
        });

        return Accepted(new
        {
            JobId = jobId,
            Message = "Employee summary report job has been queued."
        });
    }
}