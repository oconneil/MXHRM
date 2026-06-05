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
using MXHRM.Application.Common.Grid;
using MXHRM.Api.Common;
using MXHRM.Api.Authorization;

namespace MXHRM.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : BaseApiController
{
    private readonly IEmployeeService _employeeService;
    private readonly AppDbContext _db;
    private readonly IAuthorizationService _authorizationService;

    public EmployeesController(
        IEmployeeService employeeService,
        AppDbContext db,
        IAuthorizationService authorizationService
        )
    {
        _employeeService = employeeService;
        _db = db;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [Authorize(Policy = Permissions.Employee.Read)]
    [ProducesResponseType(typeof(PagedResponse<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<EmployeeResponse>>> GetAll(
    [FromQuery] GetEmployeesRequest request)
    {
        var employees = await _employeeService.GetAllAsync(request);
        return Ok(employees);
    }

    [HttpGet("{companyId}/{employeeId}")]
    [Authorize(Policy = Permissions.Employee.Read)]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById(string companyId, string employeeId)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var employee = await _employeeService.GetByIdAsync(companyId, employeeId);

        if (employee is null)
            return NotFoundError("Employee not found.");

        return Ok(employee);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Employee.Create)]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string companyId, string employeeId, UpdateEmployeeRequest request)
    {
        try
        {
            var sameCompany = await _authorizationService.AuthorizeAsync(
                User, companyId, new SameCompanyRequirement());
            if (!sameCompany.Succeeded)
                return Forbid();

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
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string companyId, string employeeId)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var deleted = await _employeeService.DeleteAsync(companyId, employeeId);

        if (!deleted)
            return NotFoundError("Employee not found.");

        return NoContent();
    }

    [HttpPost("grid")]
    [Authorize(Policy = Permissions.Employee.Read)]
    [ProducesResponseType(
    typeof(GridDataSourceResult<EmployeeResponse>),
    StatusCodes.Status200OK)]
    public async Task<ActionResult<GridDataSourceResult<EmployeeResponse>>> Grid(
    CancellationToken cancellationToken)
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

        var result = await query.ToGridDataSourceResultAsync(
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new Exception("This is a test exception.");
    }
}
