using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Auditing;
using MXHRM.Application.Auditing.DTOs;
using MXHRM.Application.Authorization;
using MXHRM.Application.Common;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Policy = Permissions.Role.Manage)]
public class AuditLogsController : BaseApiController
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<AuditLogResponse>>> GetAll(
        [FromQuery] GetAuditLogsRequest request,
        CancellationToken cancellationToken)
    {
        var auditLogs = await _auditLogService.GetAllAsync(request, cancellationToken);
        return Ok(auditLogs);
    }

    [HttpGet("{id:long}")]
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
}