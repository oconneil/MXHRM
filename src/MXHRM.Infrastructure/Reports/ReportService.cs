using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Reports;
using MXHRM.Application.Reports.DTOs;
using MXHRM.Infrastructure.Data;
using ClosedXML.Excel;
using MXHRM.Application.Reports.Exports;

namespace MXHRM.Infrastructure.Reports;

public sealed class ReportService : IReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

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
}