using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MXHRM.Api.DTOs.Employees;
using MXHRM.Api.Models;
using MXHRM.Api.Services.Employees;
using MXHRM.Api.DTOs.Common;
using Microsoft.AspNetCore.Authorization;

namespace MXHRM.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<EmployeeResponse>>> GetAll(
    [FromQuery] GetEmployeesRequest request)
    {
        var employees = await _employeeService.GetAllAsync(request);
        return Ok(employees);
    }

    [HttpGet("{companyId}/{employeeId}")]
    public async Task<ActionResult<EmployeeResponse>> GetById(
        string companyId,
        string employeeId)
    {
        var employee = await _employeeService.GetByIdAsync(companyId, employeeId);

        if (employee is null)
            return NotFound();

        return Ok(employee);
    }

    [HttpPost]
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
                return NotFound();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                message = "ข้อมูลนี้ถูกแก้ไขโดยผู้ใช้อื่นแล้ว กรุณาโหลดข้อมูลใหม่อีกครั้ง"
            });
        }
    }

    [HttpDelete("{companyId}/{employeeId}")]
    public async Task<IActionResult> Delete(
        string companyId,
        string employeeId)
    {
        var deleted = await _employeeService.DeleteAsync(companyId, employeeId);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("test-error")]
    public IActionResult TestError()
    {
        throw new Exception("This is a test exception.");
    }
}