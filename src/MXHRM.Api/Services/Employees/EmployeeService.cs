using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Data;
using MXHRM.Api.DTOs.Employees;
using MXHRM.Api.Models;
using MXHRM.Api.DTOs.Common;

namespace MXHRM.Api.Services.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
    AppDbContext db,
    ILogger<EmployeeService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResponse<EmployeeResponse>> GetAllAsync(GetEmployeesRequest request)
    {
        var query = _db.Employees
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.EmployeeID.Contains(search) ||
                x.FirstName.Contains(search) ||
                x.LastName.Contains(search) ||
                x.Email.Contains(search));
        }

        var totalItems = await query.CountAsync();

        var employees = await query
            .OrderBy(x => x.EmployeeID)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new PagedResponse<EmployeeResponse>
        {
            Items = employees.Select(ToResponse).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };
    }

    public async Task<EmployeeResponse?> GetByIdAsync(string companyId, string employeeId)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.CompanyID == companyId &&
                x.EmployeeID == employeeId);

        return employee is null ? null : ToResponse(employee);
    }

    public async Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request)
    {
        var employee = new Employee
        {
            CompanyID = request.CompanyID,
            EmployeeID = request.EmployeeID,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            HireDate = request.HireDate,
            Salary = request.Salary,
            IsActive = true,
            CreatedBy = request.CreatedBy,
            ModifiedBy = request.CreatedBy,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        _logger.LogInformation
        (
            "Employee created. CompanyID: {CompanyID}, EmployeeID: {EmployeeID}",
            employee.CompanyID,
            employee.EmployeeID
        );

        return ToResponse(employee);
    }

    public async Task<bool> UpdateAsync(
        string companyId,
        string employeeId,
        UpdateEmployeeRequest request)
    {
        var employee = await _db.Employees
            .FirstOrDefaultAsync(x =>
                x.CompanyID == companyId &&
                x.EmployeeID == employeeId);

        if (employee is null)
        {
            _logger.LogWarning
            (
                "Employee update failed. Employee not found. CompanyID: {CompanyID}, EmployeeID: {EmployeeID}",
                companyId,
                employeeId
            );

            return false;
        }

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.HireDate = request.HireDate;
        employee.Salary = request.Salary;
        employee.IsActive = request.IsActive;
        employee.ModifiedBy = request.ModifiedBy;
        employee.ModifiedDate = DateTime.UtcNow;

        _db.Entry(employee)
            .Property(x => x.RowVersion)
            .OriginalValue = request.RowVersion;

        await _db.SaveChangesAsync();

        _logger.LogInformation
        (
            "Employee updated. CompanyID: {CompanyID}, EmployeeID: {EmployeeID}",
            employee.CompanyID,
            employee.EmployeeID
        );

        return true;
    }

    public async Task<bool> DeleteAsync(string companyId, string employeeId)
    {
        var employee = await _db.Employees
            .FirstOrDefaultAsync(x =>
                x.CompanyID == companyId &&
                x.EmployeeID == employeeId);

        if (employee is null)
        {
            _logger.LogWarning
            (
                "Employee delete failed. Employee not found. CompanyID: {CompanyID}, EmployeeID: {EmployeeID}",
                companyId,
                employeeId
            );

            return false;
        }

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();

        _logger.LogInformation
        (
            "Employee deleted. CompanyID: {CompanyID}, EmployeeID: {EmployeeID}",
            companyId,
            employeeId
        );

        return true;
    }

    private static EmployeeResponse ToResponse(Employee employee)
    {
        return new EmployeeResponse
        {
            CompanyID = employee.CompanyID,
            EmployeeID = employee.EmployeeID,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            FullName = $"{employee.FirstName} {employee.LastName}",
            Email = employee.Email,
            HireDate = employee.HireDate,
            Salary = employee.Salary,
            IsActive = employee.IsActive,
            RowVersion = employee.RowVersion
        };
    }
}