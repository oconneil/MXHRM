using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MXHRM.Application.Reports.DTOs;
using MXHRM.Infrastructure.Data;

namespace MXHRM.Infrastructure.Jobs;

public class EmployeeReportJob
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmployeeReportJob> _logger;

    public EmployeeReportJob(
        AppDbContext db,
        ILogger<EmployeeReportJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        var employees = _db.Employees.AsNoTracking();

        var totalEmployees = await employees.CountAsync();
        var activeEmployees = await employees.CountAsync(x => x.IsActive);
        var inactiveEmployees = totalEmployees - activeEmployees;

        var totalSalary = totalEmployees == 0
            ? 0
            : await employees.SumAsync(x => x.Salary);

        var averageSalary = totalEmployees == 0
            ? 0
            : await employees.AverageAsync(x => x.Salary);

        var report = new EmployeeSummaryReport
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            InactiveEmployees = inactiveEmployees,
            TotalSalary = totalSalary,
            AverageSalary = averageSalary,
            GeneratedAtUtc = DateTime.UtcNow
        };

        _logger.LogInformation(
            "Employee summary report generated. TotalEmployees: {TotalEmployees}, ActiveEmployees: {ActiveEmployees}, InactiveEmployees: {InactiveEmployees}, TotalSalary: {TotalSalary}, AverageSalary: {AverageSalary}, GeneratedAtUtc: {GeneratedAtUtc}",
            report.TotalEmployees,
            report.ActiveEmployees,
            report.InactiveEmployees,
            report.TotalSalary,
            report.AverageSalary,
            report.GeneratedAtUtc);
    }
}