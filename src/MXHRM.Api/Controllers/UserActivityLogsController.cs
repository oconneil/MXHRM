using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Auditing;
using MXHRM.Application.Auditing.DTOs;
using MXHRM.Application.Authorization;
using MXHRM.Application.Common;
using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Common.Grid;
using MXHRM.Infrastructure.Common.Grid;
using MXHRM.Infrastructure.Data;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/user-activity-logs")]
[Authorize(Policy = Permissions.Activity.Read)]
public class UserActivityLogsController : BaseApiController
{
    private readonly IUserActivityLogService _userActivityLogService;
    private readonly AppDbContext _db;

    public UserActivityLogsController(
        IUserActivityLogService userActivityLogService,
        AppDbContext db)
    {
        _userActivityLogService = userActivityLogService;
        _db = db;
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

    [HttpPost("grid")]
    [Authorize(Policy = Permissions.Activity.Read)]
    public async Task<IActionResult> Grid(CancellationToken cancellationToken)
    {
        var request = GridDataSourceRequestParser.FromQuery(Request.Query);

        var query = _db.UserActivityLogs
            .AsNoTracking()
            .Select(activityLog => new UserActivityLogResponse
            {
                Id = activityLog.Id,
                ActivityType = activityLog.ActivityType,
                Description = activityLog.Description,
                Metadata = activityLog.Metadata,
                UserId = activityLog.UserId,
                UserName = activityLog.UserName,
                IpAddress = activityLog.IpAddress,
                UserAgent = activityLog.UserAgent,
                TraceId = activityLog.TraceId,
                CreatedAtUtc = activityLog.CreatedAtUtc
            });

        var result = await query.ToGridDataSourceResultAsync(request, cancellationToken);

        return Ok(result);
    }
}