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
using MXHRM.Api.Common;
using MXHRM.Application.Common.Grid;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = Permissions.Audit.Read)]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;
    private readonly AppDbContext _db;

    public AuditLogsController(
        IAuditLogService auditLogService,
        AppDbContext db)
    {
        _auditLogService = auditLogService;
        _db = db;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<AuditLogResponse>>> GetAll(
        [FromQuery] GetAuditLogsRequest request,
        CancellationToken cancellationToken)
    {
        var auditLogs = await _auditLogService.GetAllAsync(request, cancellationToken);
        return Ok(auditLogs);
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(AuditLogResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuditLogResponse>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var auditLog = await _auditLogService.GetByIdAsync(id, cancellationToken);

        if (auditLog is null)
        {
            return NotFoundError("Audit log not found.");
        }

        return Ok(auditLog);
    }

    [HttpPost("grid")]
    [Authorize(Policy = Permissions.Audit.Read)]
    [ProducesResponseType(typeof(GridDataSourceResult<AuditLogResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Grid(CancellationToken cancellationToken)
    {
        var request = GridDataSourceRequestParser.FromQuery(Request.Query);

        var query = _db.AuditLogs
            .AsNoTracking()
            .Select(auditLog => new AuditLogResponse
            {
                Id = auditLog.Id,
                TableName = auditLog.TableName,
                Action = auditLog.Action,
                KeyValues = auditLog.KeyValues,
                OldValues = auditLog.OldValues,
                NewValues = auditLog.NewValues,
                ChangedColumns = auditLog.ChangedColumns,
                UserId = auditLog.UserId,
                UserName = auditLog.UserName,
                TraceId = auditLog.TraceId,
                CreatedAtUtc = auditLog.CreatedAtUtc
            });

        var result = await query.ToGridDataSourceResultAsync(request, cancellationToken);

        return Ok(result);
    }
}