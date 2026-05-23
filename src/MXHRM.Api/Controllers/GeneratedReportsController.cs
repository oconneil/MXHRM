using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MXHRM.Api.Swagger;
using MXHRM.Application.Authorization;
using MXHRM.Application.Common;
using MXHRM.Application.Reports;
using MXHRM.Application.Reports.DTOs;

namespace MXHRM.Api.Controllers;

[ApiController]
[Route("api/generated-reports")]
[Authorize]
public sealed class GeneratedReportsController : ControllerBase
{
    private readonly IAsyncReportService _asyncReportService;

    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string PdfContentType = "application/pdf";

    public GeneratedReportsController(IAsyncReportService asyncReportService)
    {
        _asyncReportService = asyncReportService;
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Report.Manage)]
    [ProducesResponseType(typeof(GeneratedReportResponse), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<GeneratedReportResponse>> Create(
        [FromBody] CreateGeneratedReportRequest request,
        CancellationToken cancellationToken)
    {
        var generatedReport = await _asyncReportService.CreateAsync(
            request,
            cancellationToken);

        return AcceptedAtAction(
            nameof(GetById),
            new { id = generatedReport.Id },
            generatedReport);
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Report.Manage)]
    public async Task<ActionResult<PagedResponse<GeneratedReportResponse>>> GetAll(
        [FromQuery] GeneratedReportListRequest request,
        CancellationToken cancellationToken)
    {
        var generatedReports = await _asyncReportService.GetAllAsync(
            request,
            cancellationToken);

        return Ok(generatedReports);
    }

    [HttpGet("{id:long}")]
    [Authorize(Policy = Permissions.Report.Manage)]
    public async Task<ActionResult<GeneratedReportResponse>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var generatedReport = await _asyncReportService.GetByIdAsync(
            id,
            cancellationToken);

        if (generatedReport is null)
        {
            return NotFound();
        }

        return Ok(generatedReport);
    }

    [HttpGet("{id:long}/download")]
    [Authorize(Policy = Permissions.Report.Manage)]
    [ProducesFile(ExcelContentType, PdfContentType)]
    public async Task<IActionResult> Download(
        long id,
        CancellationToken cancellationToken)
    {
        var file = await _asyncReportService.DownloadAsync(
            id,
            cancellationToken);

        if (file is null)
        {
            return NotFound();
        }

        return File(
            file.Content,
            file.ContentType,
            file.FileName);
    }
}
