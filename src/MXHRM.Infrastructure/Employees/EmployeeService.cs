using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MXHRM.Infrastructure.Data;
using MXHRM.Application.Common;
using MXHRM.Application.Employees;
using MXHRM.Application.Employees.DTOs;
using MXHRM.Domain.Employees;
using System.Text.Json;
using MXHRM.Application.Common.Interfaces;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using MXHRM.Application.Auditing;
using MXHRM.Application.Auditing.DTOs;

namespace MXHRM.Infrastructure.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;
    private readonly ILogger<EmployeeService> _logger;
    private readonly ICacheService _cache;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogService _auditLogService;

    public EmployeeService(
    AppDbContext db,
    ILogger<EmployeeService> logger,
    ICacheService cache,
    IConfiguration configuration,
    IAuditLogService auditLogService)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
        _configuration = configuration;
        _auditLogService = auditLogService;
    }

    private static readonly JsonSerializerOptions AuditJsonOptions = new()
    {
        WriteIndented = false
    };

    public async Task<PagedResponse<EmployeeResponse>> GetAllAsync(GetEmployeesRequest request)
    {
        var cacheKey = GetEmployeeListCacheKey(request);

        var cachedEmployees = await _cache.GetAsync<PagedResponse<EmployeeResponse>>(cacheKey);

        if (cachedEmployees is not null)
        {
            _logger.LogInformation("Employee list returned from cache. Key: {CacheKey}", cacheKey);
            return cachedEmployees;
        }

        var stopwatch = Stopwatch.StartNew();

        var query = _db.Employees
            .AsNoTracking()
            .AsQueryable();

        query = ApplyFilters(query, request);

        var totalItems = await query.CountAsync();

        var employeesQuery = ApplySorting(ProjectToResponse(query), request);

        var employees = await employeesQuery
            .Skip(GetSkipCount(request))
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

        stopwatch.Stop();

        var slowQueryThresholdMs = _configuration.GetValue<int>("Performance:SlowQueryThresholdMs", 500);

        if (stopwatch.ElapsedMilliseconds >= slowQueryThresholdMs)
        {
            _logger.LogWarning(
                "Slow employee list query detected. Elapsed: {ElapsedMilliseconds} ms, Threshold: {ThresholdMs} ms, Page: {Page}, PageSize: {PageSize}, Search: {Search}",
                stopwatch.ElapsedMilliseconds,
                slowQueryThresholdMs,
                request.Page,
                request.PageSize,
                request.Search);
        }
        else
        {
            _logger.LogInformation(
                "Employee list database query completed in {ElapsedMilliseconds} ms. Page: {Page}, PageSize: {PageSize}, Search: {Search}",
                stopwatch.ElapsedMilliseconds,
                request.Page,
                request.PageSize,
                request.Search);
        }


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
            EmployeeID = request.EmployeeID,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            HireDate = request.HireDate,
            Salary = request.Salary,
            IsActive = true
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        // Log the creation of the employee for auditing
        await _auditLogService.LogAsync(new CreateAuditLogRequest
        {
            TableName = "Employees",
            Action = "Insert",
            KeyValues = GetEmployeeAuditKey(employee.CompanyID, employee.EmployeeID),
            NewValues = SerializeAuditValue(ToResponse(employee)),
            ChangedColumns = SerializeAuditValue(new[]
            {
                nameof(Employee.CompanyID),
                nameof(Employee.EmployeeID),
                nameof(Employee.FirstName),
                nameof(Employee.LastName),
                nameof(Employee.Email),
                nameof(Employee.HireDate),
                nameof(Employee.Salary),
                nameof(Employee.IsActive)
            })
        });

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

        var oldValues = ToResponse(employee);
        var changedColumns = GetChangedColumns(employee, request);

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.HireDate = request.HireDate;
        employee.Salary = request.Salary;
        employee.IsActive = request.IsActive;

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

        var newValues = ToResponse(employee);

        await _auditLogService.LogAsync(new CreateAuditLogRequest
        {
            TableName = "Employees",
            Action = "Update",
            KeyValues = GetEmployeeAuditKey(companyId, employeeId),
            OldValues = SerializeAuditValue(oldValues),
            NewValues = SerializeAuditValue(newValues),
            ChangedColumns = changedColumns
        });

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

        var oldValues = ToResponse(employee);

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();

        await _auditLogService.LogAsync(new CreateAuditLogRequest
        {
            TableName = "Employees",
            Action = "Delete",
            KeyValues = GetEmployeeAuditKey(companyId, employeeId),
            OldValues = SerializeAuditValue(oldValues),
            ChangedColumns = SerializeAuditValue(new[]
            {
                nameof(Employee.CompanyID),
                nameof(Employee.EmployeeID),
                nameof(Employee.FirstName),
                nameof(Employee.LastName),
                nameof(Employee.Email),
                nameof(Employee.HireDate),
                nameof(Employee.Salary),
                nameof(Employee.IsActive)
            })
        });
        
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

        var companyId = string.IsNullOrWhiteSpace(request.CompanyID)
            ? "all"
            : request.CompanyID.Trim().ToLowerInvariant();

        var isActive = request.IsActive.HasValue
            ? request.IsActive.Value.ToString().ToLowerInvariant()
            : "all";

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy)
            ? "employeeid"
            : request.SortBy.Trim().ToLowerInvariant();

        var sortDirection = string.IsNullOrWhiteSpace(request.SortDirection)
            ? "asc"
            : request.SortDirection.Trim().ToLowerInvariant();

        return $"employees:list:company={companyId}:active={isActive}:page={request.Page}:size={request.PageSize}:search={search}:sort={sortBy}:direction={sortDirection}";
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

    private static int GetSkipCount(GetEmployeesRequest request)
    {
        return (request.Page - 1) * request.PageSize;
    }

    private static IQueryable<Employee> ApplyFilters(
    IQueryable<Employee> query,
    GetEmployeesRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.CompanyID))
        {
            query = query.Where(x => x.CompanyID == request.CompanyID.Trim());
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();

            query = query.Where(x =>
                x.EmployeeID.Contains(search) ||
                x.FirstName.Contains(search) ||
                x.LastName.Contains(search) ||
                x.Email.Contains(search));
        }

        return query;
    }

    private static IQueryable<EmployeeResponse> ApplySorting(
    IQueryable<EmployeeResponse> query,
    GetEmployeesRequest request)
    {
        var sortBy = request.SortBy?.Trim().ToLowerInvariant() ?? "employeeid";
        var sortDirection = request.SortDirection?.Trim().ToLowerInvariant() ?? "asc";
        var isDescending = sortDirection == "desc";

        return sortBy switch
        {
            "firstname" => isDescending
                ? query.OrderByDescending(x => x.FirstName).ThenBy(x => x.EmployeeID)
                : query.OrderBy(x => x.FirstName).ThenBy(x => x.EmployeeID),

            "lastname" => isDescending
                ? query.OrderByDescending(x => x.LastName).ThenBy(x => x.EmployeeID)
                : query.OrderBy(x => x.LastName).ThenBy(x => x.EmployeeID),

            "email" => isDescending
                ? query.OrderByDescending(x => x.Email).ThenBy(x => x.EmployeeID)
                : query.OrderBy(x => x.Email).ThenBy(x => x.EmployeeID),

            "hiredate" => isDescending
                ? query.OrderByDescending(x => x.HireDate).ThenBy(x => x.EmployeeID)
                : query.OrderBy(x => x.HireDate).ThenBy(x => x.EmployeeID),

            "salary" => isDescending
                ? query.OrderByDescending(x => x.Salary).ThenBy(x => x.EmployeeID)
                : query.OrderBy(x => x.Salary).ThenBy(x => x.EmployeeID),

            _ => isDescending
                ? query.OrderByDescending(x => x.CompanyID).ThenByDescending(x => x.EmployeeID)
                : query.OrderBy(x => x.CompanyID).ThenBy(x => x.EmployeeID)
        };
    }

    private static string SerializeAuditValue(object value)
    {
        return JsonSerializer.Serialize(value, AuditJsonOptions);
    }

    private static string GetEmployeeAuditKey(string companyId, string employeeId)
    {
        return SerializeAuditValue(new
        {
            CompanyID = companyId,
            EmployeeID = employeeId
        });
    }

    private static string GetChangedColumns(
        Employee employee,
        UpdateEmployeeRequest request)
    {
        var changedColumns = new List<string>();

        if (employee.FirstName != request.FirstName) changedColumns.Add(nameof(Employee.FirstName));
        if (employee.LastName != request.LastName) changedColumns.Add(nameof(Employee.LastName));
        if (employee.Email != request.Email) changedColumns.Add(nameof(Employee.Email));
        if (employee.HireDate != request.HireDate) changedColumns.Add(nameof(Employee.HireDate));
        if (employee.Salary != request.Salary) changedColumns.Add(nameof(Employee.Salary));
        if (employee.IsActive != request.IsActive) changedColumns.Add(nameof(Employee.IsActive));

        return SerializeAuditValue(changedColumns);
    }

}
