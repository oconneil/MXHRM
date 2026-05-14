using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MXHRM.Infrastructure.Data;
using MXHRM.Application.Common;
using MXHRM.Application.Employees;
using MXHRM.Application.Employees.DTOs;
using MXHRM.Domain.Employees;
using System.Text.Json;
using MXHRM.Application.Common.Interfaces;

namespace MXHRM.Infrastructure.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmployeeService> _logger;
    private readonly ICacheService _cache;

    public EmployeeService(
    AppDbContext db,
    ILogger<EmployeeService> logger,
    ICacheService cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }


    public async Task<PagedResponse<EmployeeResponse>> GetAllAsync(GetEmployeesRequest request)
    {
        var cacheKey = GetEmployeeListCacheKey(request);

        var cachedEmployees = await _cache.GetAsync<PagedResponse<EmployeeResponse>>(cacheKey);

        if (cachedEmployees is not null)
        {
            _logger.LogInformation("Employee list returned from cache. Key: {CacheKey}", cacheKey);
            return cachedEmployees;
        }

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

        var employees = await ProjectToResponse(query)
            .OrderBy(x => x.EmployeeID)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var response = new PagedResponse<EmployeeResponse>
        {
            Items = employees,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
        };


        await _cache.SetAsync(
            cacheKey,
            response,
            TimeSpan.FromMinutes(5));

        _logger.LogInformation("Employee list saved to cache. Key: {CacheKey}", cacheKey);

        return response;
    }


    public async Task<EmployeeResponse?> GetByIdAsync(string companyId, string employeeId)
    {
        var cacheKey = GetEmployeeDetailCacheKey(companyId, employeeId);

        var cachedEmployee = await _cache.GetAsync<EmployeeResponse>(cacheKey);

        if (cachedEmployee is not null)
        {
            _logger.LogInformation("Employee detail returned from cache. Key: {CacheKey}", cacheKey);
            return cachedEmployee;
        }

        var response = await ProjectToResponse(_db.Employees.AsNoTracking())
            .FirstOrDefaultAsync(x => x.CompanyID == companyId && x.EmployeeID == employeeId);

        if (response is null)
        {
            return null;
        }

        await _cache.SetAsync(
            cacheKey,
            response,
            TimeSpan.FromMinutes(5));

        _logger.LogInformation("Employee detail saved to cache. Key: {CacheKey}", cacheKey);

        return response;
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

        await ClearEmployeeCacheAsync(
            employee.CompanyID,
            employee.EmployeeID);


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

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(
                "This employee was updated by another user. Please reload and try again.",
                ex);
        }

        await ClearEmployeeCacheAsync(companyId, employeeId);

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

        await ClearEmployeeCacheAsync(companyId, employeeId);

        _logger.LogInformation
        (
            "Employee deleted. CompanyID: {CompanyID}, EmployeeID: {EmployeeID}",
            companyId,
            employeeId
        );

        return true;
    }

    private static string GetEmployeeListCacheKey(GetEmployeesRequest request)
    {
        var search = string.IsNullOrWhiteSpace(request.Search)
            ? "all"
            : request.Search.Trim().ToLowerInvariant();

        return $"employees:list:page={request.Page}:size={request.PageSize}:search={search}";
    }


    private static string GetEmployeeDetailCacheKey(string companyId, string employeeId)
    {
        return $"employees:detail:company={companyId}:employee={employeeId}";
    }

    private const string EmployeeListCachePrefix = "employees:list:";

    private async Task ClearEmployeeCacheAsync(string companyId, string employeeId, CancellationToken cancellationToken = default)
    {
        var detailCacheKey = GetEmployeeDetailCacheKey(companyId, employeeId);

        await _cache.RemoveAsync(detailCacheKey, cancellationToken);
        await _cache.RemoveByPrefixAsync(EmployeeListCachePrefix, cancellationToken);

        _logger.LogInformation(
            "Employee cache cleared. DetailKey: {DetailCacheKey}, ListPrefix: {ListPrefix}",
            detailCacheKey,
            EmployeeListCachePrefix);
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

    private static IQueryable<EmployeeResponse> ProjectToResponse(
    IQueryable<Employee> query)
    {
        return query.Select(x => new EmployeeResponse
        {
            CompanyID = x.CompanyID,
            EmployeeID = x.EmployeeID,
            FirstName = x.FirstName,
            LastName = x.LastName,
            FullName = x.FirstName + " " + x.LastName,
            Email = x.Email,
            HireDate = x.HireDate,
            Salary = x.Salary,
            IsActive = x.IsActive,
            RowVersion = x.RowVersion
        });
    }

}
