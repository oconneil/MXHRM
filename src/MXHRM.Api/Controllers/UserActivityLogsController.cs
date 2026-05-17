using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Auditing;
using MXHRM.Application.Auditing.DTOs;
using MXHRM.Application.Authorization;
using MXHRM.Application.Common;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/user-activity-logs")]
[Authorize(Policy = Permissions.Activity.Read)]
public class UserActivityLogsController : BaseApiController
{
    private readonly IUserActivityLogService _userActivityLogService;

    public UserActivityLogsController(IUserActivityLogService userActivityLogService)
    {
        _userActivityLogService = userActivityLogService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserActivityLogResponse>>> GetAll(
        [FromQuery] GetUserActivityLogsRequest request,
        CancellationToken cancellationToken)
    {
        var activityLogs = await _userActivityLogService.GetAllAsync(request, cancellationToken);
        return Ok(activityLogs);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<UserActivityLogResponse>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var activityLog = await _userActivityLogService.GetByIdAsync(id, cancellationToken);

        if (activityLog is null)
        {
            return NotFoundError("User activity log not found.");
        }

        return Ok(activityLog);
    }
}