using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Reports;
using MXHRM.Application.Reports.DTOs;
using MXHRM.Infrastructure.Data;
using ClosedXML.Excel;
using MXHRM.Application.Reports.Exports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MXHRM.Infrastructure.Reports;

public sealed class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private const string ExcelContentType =
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const string PdfContentType = "application/pdf";

    public async Task<EmployeeSummaryReportResponse> GetEmployeeSummaryAsync(
        EmployeeSummaryReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.CompanyID))
        {
            query = query.Where(employee => employee.CompanyID == request.CompanyID);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(employee => employee.IsActive == request.IsActive.Value);
        }

        if (request.HireDateFrom.HasValue)
        {
            query = query.Where(employee => employee.HireDate >= request.HireDateFrom.Value);
        }

        if (request.HireDateTo.HasValue)
        {
            query = query.Where(employee => employee.HireDate <= request.HireDateTo.Value);
        }

        var totalEmployees = await query.CountAsync(cancellationToken);
        var activeEmployees = await query.CountAsync(
            employee => employee.IsActive,
            cancellationToken);
        var inactiveEmployees = totalEmployees - activeEmployees;

        var salarySummary = await query
            .GroupBy(_ => 1)
            .Select(group => new
            {
                AverageSalary = group.Average(employee => employee.Salary),
                TotalSalary = group.Sum(employee => employee.Salary)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var byCompany = await query
            .GroupBy(employee => employee.CompanyID)
            .Select(group => new EmployeeSummaryByCompanyResponse
            {
                CompanyID = group.Key,
                TotalEmployees = group.Count(),
                ActiveEmployees = group.Count(employee => employee.IsActive),
                InactiveEmployees = group.Count(employee => !employee.IsActive),
                AverageSalary = group.Average(employee => employee.Salary),
                TotalSalary = group.Sum(employee => employee.Salary)
            })
            .OrderBy(item => item.CompanyID)
            .ToListAsync(cancellationToken);

        return new EmployeeSummaryReportResponse
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            InactiveEmployees = inactiveEmployees,
            AverageSalary = salarySummary?.AverageSalary ?? 0,
            TotalSalary = salarySummary?.TotalSalary ?? 0,
            GeneratedAtUtc = DateTime.UtcNow,
            ByCompany = byCompany
        };
    }

    public async Task<ReportFileResponse> ExportEmployeeSummaryExcelAsync(
    EmployeeSummaryReportRequest request,
    CancellationToken cancellationToken = default)
    {
        var report = await GetEmployeeSummaryAsync(request, cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Employee Summary");

        worksheet.Cell(1, 1).Value = "Employee Summary Report";
        worksheet.Range(1, 1, 1, 6).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;

        worksheet.Cell(2, 1).Value = "Generated At";
        worksheet.Cell(2, 2).Value = report.GeneratedAtUtc;

        worksheet.Cell(4, 1).Value = "Total Employees";
        worksheet.Cell(4, 2).Value = report.TotalEmployees;

        worksheet.Cell(5, 1).Value = "Active Employees";
        worksheet.Cell(5, 2).Value = report.ActiveEmployees;

        worksheet.Cell(6, 1).Value = "Inactive Employees";
        worksheet.Cell(6, 2).Value = report.InactiveEmployees;

        worksheet.Cell(7, 1).Value = "Average Salary";
        worksheet.Cell(7, 2).Value = report.AverageSalary;

        worksheet.Cell(8, 1).Value = "Total Salary";
        worksheet.Cell(8, 2).Value = report.TotalSalary;

        worksheet.Range(4, 1, 8, 1).Style.Font.Bold = true;
        worksheet.Range(7, 2, 8, 2).Style.NumberFormat.Format = "#,##0.00";

        var tableStartRow = 11;

        worksheet.Cell(tableStartRow, 1).Value = "Company ID";
        worksheet.Cell(tableStartRow, 2).Value = "Total Employees";
        worksheet.Cell(tableStartRow, 3).Value = "Active Employees";
        worksheet.Cell(tableStartRow, 4).Value = "Inactive Employees";
        worksheet.Cell(tableStartRow, 5).Value = "Average Salary";
        worksheet.Cell(tableStartRow, 6).Value = "Total Salary";

        var headerRange = worksheet.Range(tableStartRow, 1, tableStartRow, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0f766e");
        headerRange.Style.Font.FontColor = XLColor.White;

        var row = tableStartRow + 1;

        foreach (var item in report.ByCompany)
        {
            worksheet.Cell(row, 1).Value = item.CompanyID;
            worksheet.Cell(row, 2).Value = item.TotalEmployees;
            worksheet.Cell(row, 3).Value = item.ActiveEmployees;
            worksheet.Cell(row, 4).Value = item.InactiveEmployees;
            worksheet.Cell(row, 5).Value = item.AverageSalary;
            worksheet.Cell(row, 6).Value = item.TotalSalary;

            row++;
        }

        if (report.ByCompany.Count > 0)
        {
            var dataRange = worksheet.Range(tableStartRow, 1, row - 1, 6);
            dataRange.CreateTable();
            worksheet.Range(tableStartRow + 1, 5, row - 1, 6)
                .Style.NumberFormat.Format = "#,##0.00";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new ReportFileResponse
        {
            Content = stream.ToArray(),
            ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"employee-summary-report-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        };
    }

    public async Task<AuditReportResponse> GetAuditReportAsync(
    AuditReportRequest request,
    CancellationToken cancellationToken)
    {
        var query = _db.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.TableName))
        {
            query = query.Where(x => x.TableName == request.TableName.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x => x.Action == request.Action.Trim());
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            query = query.Where(x => x.UserId == request.UserId.Trim());
        }

        if (request.FromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= request.ToUtc.Value);
        }

        var totalAuditLogs = await query.CountAsync(cancellationToken);

        var byAction = await query
            .GroupBy(x => x.Action)
            .Select(g => new AuditActionSummaryResponse
            {
                Action = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var byTable = await query
            .GroupBy(x => x.TableName)
            .Select(g => new AuditTableSummaryResponse
            {
                TableName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var byUser = await query
            .GroupBy(x => new { x.UserId, x.UserName })
            .Select(g => new AuditUserSummaryResponse
            {
                UserId = g.Key.UserId,
                UserName = g.Key.UserName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        return new AuditReportResponse
        {
            TotalAuditLogs = totalAuditLogs,
            GeneratedAtUtc = DateTime.UtcNow,
            ByAction = byAction,
            ByTable = byTable,
            ByUser = byUser
        };
    }

    public async Task<ReportFileResponse> ExportAuditReportExcelAsync(
    AuditReportRequest request,
    CancellationToken cancellationToken)
    {
        var report = await GetAuditReportAsync(request, cancellationToken);

        using var workbook = new XLWorkbook();

        var summaryWorksheet = workbook.Worksheets.Add("Summary");

        summaryWorksheet.Cell(1, 1).Value = "Audit Report";
        summaryWorksheet.Cell(2, 1).Value = "Generated At UTC";
        summaryWorksheet.Cell(2, 2).Value = report.GeneratedAtUtc;

        summaryWorksheet.Cell(4, 1).Value = "Total Audit Logs";
        summaryWorksheet.Cell(4, 2).Value = report.TotalAuditLogs;

        summaryWorksheet.Columns().AdjustToContents();

        var byActionWorksheet = workbook.Worksheets.Add("By Action");

        byActionWorksheet.Cell(1, 1).Value = "Action";
        byActionWorksheet.Cell(1, 2).Value = "Count";

        for (var i = 0; i < report.ByAction.Count; i++)
        {
            var row = i + 2;
            var item = report.ByAction[i];

            byActionWorksheet.Cell(row, 1).Value = item.Action;
            byActionWorksheet.Cell(row, 2).Value = item.Count;
        }

        byActionWorksheet.Columns().AdjustToContents();

        var byTableWorksheet = workbook.Worksheets.Add("By Table");

        byTableWorksheet.Cell(1, 1).Value = "Table Name";
        byTableWorksheet.Cell(1, 2).Value = "Count";

        for (var i = 0; i < report.ByTable.Count; i++)
        {
            var row = i + 2;
            var item = report.ByTable[i];

            byTableWorksheet.Cell(row, 1).Value = item.TableName;
            byTableWorksheet.Cell(row, 2).Value = item.Count;
        }

        byTableWorksheet.Columns().AdjustToContents();

        var byUserWorksheet = workbook.Worksheets.Add("By User");

        byUserWorksheet.Cell(1, 1).Value = "User ID";
        byUserWorksheet.Cell(1, 2).Value = "User Name";
        byUserWorksheet.Cell(1, 3).Value = "Count";

        for (var i = 0; i < report.ByUser.Count; i++)
        {
            var row = i + 2;
            var item = report.ByUser[i];

            byUserWorksheet.Cell(row, 1).Value = item.UserId ?? "-";
            byUserWorksheet.Cell(row, 2).Value = item.UserName ?? "-";
            byUserWorksheet.Cell(row, 3).Value = item.Count;
        }

        byUserWorksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new ReportFileResponse
        {
            Content = stream.ToArray(),
            ContentType =
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            FileName = $"audit-report-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
        };
    }

    public async Task<ReportFileResponse> ExportEmployeeSummaryPdfAsync(
    EmployeeSummaryReportRequest request,
    CancellationToken cancellationToken)
    {
        var report = await GetEmployeeSummaryAsync(request, cancellationToken);

        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(40);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Column(column =>
                    {
                        column.Item()
                            .Text("Employee Summary Report")
                            .FontSize(20)
                            .Bold();

                        column.Item()
                            .Text($"Generated at UTC: {report.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Spacing(14);

                        column.Item()
                            .Row(row =>
                            {
                                row.RelativeItem().Element(container =>
                                    BuildSummaryBox(container, "Total Employees", report.TotalEmployees.ToString("N0")));

                                row.RelativeItem().Element(container =>
                                    BuildSummaryBox(container, "Active Employees", report.ActiveEmployees.ToString("N0")));

                                row.RelativeItem().Element(container =>
                                    BuildSummaryBox(container, "Inactive Employees", report.InactiveEmployees.ToString("N0")));
                            });

                        column.Item()
                            .Row(row =>
                            {
                                row.RelativeItem().Element(container =>
                                    BuildSummaryBox(container, "Average Salary", report.AverageSalary.ToString("N2")));

                                row.RelativeItem().Element(container =>
                                    BuildSummaryBox(container, "Total Salary", report.TotalSalary.ToString("N2")));
                            });

                        column.Item()
                            .Text("Breakdown by Company")
                            .FontSize(14)
                            .Bold();

                        column.Item()
                            .Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(BuildTableHeader).Text("Company");
                                    header.Cell().Element(BuildTableHeader).AlignRight().Text("Total");
                                    header.Cell().Element(BuildTableHeader).AlignRight().Text("Active");
                                    header.Cell().Element(BuildTableHeader).AlignRight().Text("Inactive");
                                    header.Cell().Element(BuildTableHeader).AlignRight().Text("Avg Salary");
                                    header.Cell().Element(BuildTableHeader).AlignRight().Text("Total Salary");
                                });

                                foreach (var item in report.ByCompany)
                                {
                                    table.Cell().Element(BuildTableCell).Text(item.CompanyID);
                                    table.Cell().Element(BuildTableCell).AlignRight().Text(item.TotalEmployees.ToString("N0"));
                                    table.Cell().Element(BuildTableCell).AlignRight().Text(item.ActiveEmployees.ToString("N0"));
                                    table.Cell().Element(BuildTableCell).AlignRight().Text(item.InactiveEmployees.ToString("N0"));
                                    table.Cell().Element(BuildTableCell).AlignRight().Text(item.AverageSalary.ToString("N2"));
                                    table.Cell().Element(BuildTableCell).AlignRight().Text(item.TotalSalary.ToString("N2"));
                                }
                            });
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" of ");
                        text.TotalPages();
                    });
            });
        }).GeneratePdf();

        return new ReportFileResponse
        {
            Content = pdfBytes,
            ContentType = PdfContentType,
            FileName = $"employee-summary-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
        };
    }

    private static void BuildSummaryBox(
    IContainer container,
    string label,
    string value)
    {
        container
            .Border(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Background(Colors.Grey.Lighten5)
            .Padding(10)
            .Column(column =>
            {
                column.Item()
                    .Text(label)
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);

                column.Item()
                    .Text(value)
                    .FontSize(14)
                    .Bold();
            });
    }

    private static IContainer BuildTableHeader(IContainer container)
    {
        return container
            .Background(Colors.Blue.Darken2)
            .DefaultTextStyle(x => x.FontColor(Colors.White).Bold())
            .PaddingVertical(6)
            .PaddingHorizontal(5);
    }

    private static IContainer BuildTableCell(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .PaddingVertical(5)
            .PaddingHorizontal(5);
    }
}