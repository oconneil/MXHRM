using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}