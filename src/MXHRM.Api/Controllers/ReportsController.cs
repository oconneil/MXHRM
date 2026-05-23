using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Api.Swagger;
using MXHRM.Application.Authorization;
using MXHRM.Application.Reports;
using MXHRM.Application.Reports.DTOs;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    private const string ExcelContentType =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpGet("employee-summary")]
    [Authorize(Policy = Permissions.Employee.Read)]
    public async Task<ActionResult<EmployeeSummaryReportResponse>> GetEmployeeSummary(
        [FromQuery] EmployeeSummaryReportRequest request,
        CancellationToken cancellationToken)
    {
        var report = await _reportService.GetEmployeeSummaryAsync(
            request,
            cancellationToken);

        return Ok(report);
    }

    [HttpGet("employee-summary/export/excel")]
    [Authorize(Policy = Permissions.Employee.Read)]
    [ProducesFile(ExcelContentType)]
    public async Task<IActionResult> ExportEmployeeSummaryExcel(
    [FromQuery] EmployeeSummaryReportRequest request,
    CancellationToken cancellationToken)
    {
        var file = await _reportService.ExportEmployeeSummaryExcelAsync(
            request,
            cancellationToken);

        return File(
            file.Content,
            file.ContentType,
            file.FileName);
    }
    
    [HttpGet("audit")]
    [Authorize(Policy = Permissions.Audit.Read)]
    public async Task<ActionResult<AuditReportResponse>> GetAuditReport(
    [FromQuery] AuditReportRequest request,
    CancellationToken cancellationToken)
    {
        var report = await _reportService.GetAuditReportAsync(
            request,
            cancellationToken);

        return Ok(report);
    }
}