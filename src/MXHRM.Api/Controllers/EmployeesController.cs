using Microsoft.AspNetCore.Mvc;
using MXHRM.Application.Employees;
using Microsoft.AspNetCore.Authorization;
using MXHRM.Application.Authorization;
using MXHRM.Application.Common;
using MXHRM.Application.Employees.DTOs;
// using Kendo.Mvc.Extensions;
// using Kendo.Mvc.UI;
using MXHRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Common.Grid;
using MXHRM.Infrastructure.Common.Grid;

namespace MXHRM.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : BaseApiController
{
    private readonly IEmployeeService _employeeService;
    private readonly AppDbContext _db;

    public EmployeesController(
        IEmployeeService employeeService,
        AppDbContext db)
    {
        _employeeService = employeeService;
        _db = db;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Employee.Read)]
    public async Task<ActionResult<PagedResponse<EmployeeResponse>>> GetAll(
    [FromQuery] GetEmployeesRequest request)
    {
        var employees = await _employeeService.GetAllAsync(request);
        return Ok(employees);
    }

    [HttpGet("{companyId}/{employeeId}")]
    [Authorize(Policy = Permissions.Employee.Read)]
    public async Task<ActionResult<EmployeeResponse>> GetById(
        string companyId,
        string employeeId)
    {
        var employee = await _employeeService.GetByIdAsync(companyId, employeeId);

        if (employee is null)
            return NotFoundError("Employee not found.");

        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Employee.Create)]
    public async Task<ActionResult<EmployeeResponse>> Create(CreateEmployeeRequest request)
    {
        var employee = await _employeeService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                companyId = employee.CompanyID,
                employeeId = employee.EmployeeID
            },
            employee);
    }

    [HttpPut("{companyId}/{employeeId}")]
    [Authorize(Policy = Permissions.Employee.Update)]
    public async Task<IActionResult> Update(
        string companyId,
        string employeeId,
        UpdateEmployeeRequest request)
    {
        try
        {
            var updated = await _employeeService.UpdateAsync(
                companyId,
                employeeId,
                request);

            if (!updated)
                return NotFoundError("Employee not found.");

            return NoContent();
        }
        catch (ConcurrencyConflictException)
        {
            return ConflictError("ข้อมูลนี้ถูกแก้ไขโดยผู้ใช้อื่นแล้ว กรุณาโหลดข้อมูลใหม่อีกครั้ง");
        }
    }

    [HttpDelete("{companyId}/{employeeId}")]
    [Authorize(Policy = Permissions.Employee.Delete)]
    public async Task<IActionResult> Delete(
        string companyId,
        string employeeId)
    {
        var deleted = await _employeeService.DeleteAsync(companyId, employeeId);

        if (!deleted)
            return NotFoundError("Employee not found.");

        return NoContent();
    }

    [HttpPost("grid")]
    public async Task<IActionResult> Grid(CancellationToken cancellationToken)
    {
        var request = GridDataSourceRequestParser.FromQuery(Request.Query);

        var query = _db.Employees
            .AsNoTracking()
            .Select(employee => new EmployeeResponse
            {
                CompanyID = employee.CompanyID,
                EmployeeID = employee.EmployeeID,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                FullName = $"{employee.FirstName} {employee.LastName}",
                Email = employee.Email,
                HireDate = employee.HireDate,
                Salary = employee.Salary,
                IsActive = employee.IsActive
            });

        var result = await query.ToGridDataSourceResultAsync(request, cancellationToken);

        return Ok(result);
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new Exception("This is a test exception.");
    }
}
