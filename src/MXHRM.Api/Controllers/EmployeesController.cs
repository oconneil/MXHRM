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
using Microsoft.Extensions.Options;

namespace MXHRM.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : BaseApiController
{
    private readonly IEmployeeService _employeeService;
    private readonly AppDbContext _db;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEmployeeFileService _employeeFileService;
    private readonly FileUploadOptions _fileUploadOptions;

    public EmployeesController(
        IEmployeeService employeeService,
        AppDbContext db,
        IAuthorizationService authorizationService,
        IEmployeeFileService employeeFileService,
        IOptions<FileUploadOptions> fileUploadOptions
        )
    {
        _employeeService = employeeService;
        _db = db;
        _authorizationService = authorizationService;
        _employeeFileService = employeeFileService;
        _fileUploadOptions = fileUploadOptions.Value;
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

    [HttpPost("{companyId}/{employeeId}/photo")]
    [Authorize(Policy = Permissions.Employee.Update)]
    [RequestSizeLimitFromConfig("Photo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhoto(string companyId, string employeeId, IFormFile file)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var error = FileUploadValidation.Validate(file, _fileUploadOptions.Photo);
        if (error is not null)
            return BadRequestError(error);

        await using var stream = file.OpenReadStream();
        var key = await _employeeFileService.SetPhotoAsync(companyId, employeeId, stream, file.FileName);

        if (key is null)
            return NotFoundError("Employee not found.");

        return Ok(new { photoPath = key });
    }

    [HttpGet("{companyId}/{employeeId}/photo")]
    [Authorize(Policy = Permissions.Employee.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPhoto(string companyId, string employeeId)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var photo = await _employeeFileService.GetPhotoAsync(companyId, employeeId);
        if (photo is null)
            return NotFound();

        // ส่ง stream กลับให้ browser แสดง inline (ไม่โหลดทั้งไฟล์เข้า memory)
        return File(photo.Content, photo.ContentType);
    }

    [HttpDelete("{companyId}/{employeeId}/photo")]
    [Authorize(Policy = Permissions.Employee.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhoto(string companyId, string employeeId)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var deleted = await _employeeFileService.DeletePhotoAsync(companyId, employeeId);
        if (!deleted)
            return NotFoundError("Employee not found.");

        return NoContent();
    }

    [HttpPost("{companyId}/{employeeId}/documents")]
    [Authorize(Policy = Permissions.Employee.Update)]
    [RequestSizeLimitFromConfig("Document")]
    [ProducesResponseType(typeof(EmployeeDocumentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadDocument(
    string companyId, string employeeId, IFormFile file, [FromForm] string documentType)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var error = FileUploadValidation.Validate(file, _fileUploadOptions.Document);
        if (error is not null)
            return BadRequestError(error);

        await using var stream = file.OpenReadStream();
        var doc = await _employeeFileService.AddDocumentAsync(
            companyId, employeeId, stream, file.FileName, file.ContentType, file.Length, documentType ?? string.Empty);

        if (doc is null)
            return NotFoundError("Employee not found.");

        return CreatedAtAction(
            nameof(GetDocument),
            new { companyId, employeeId, documentId = doc.Id },
            doc);
    }

    [HttpGet("{companyId}/{employeeId}/documents")]
    [Authorize(Policy = Permissions.Employee.Read)]
    [ProducesResponseType(typeof(IReadOnlyList<EmployeeDocumentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListDocuments(string companyId, string employeeId)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var docs = await _employeeFileService.ListDocumentsAsync(companyId, employeeId);
        return Ok(docs);
    }

    [HttpGet("{companyId}/{employeeId}/documents/{documentId:guid}")]
    [Authorize(Policy = Permissions.Employee.Read)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(string companyId, string employeeId, Guid documentId)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var doc = await _employeeFileService.GetDocumentAsync(companyId, employeeId, documentId);
        if (doc is null)
            return NotFound();

        return File(doc.Content, doc.ContentType, doc.FileName);
    }

    [HttpDelete("{companyId}/{employeeId}/documents/{documentId:guid}")]
    [Authorize(Policy = Permissions.Employee.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(string companyId, string employeeId, Guid documentId)
    {
        var sameCompany = await _authorizationService.AuthorizeAsync(
            User, companyId, new SameCompanyRequirement());
        if (!sameCompany.Succeeded)
            return Forbid();

        var deleted = await _employeeFileService.DeleteDocumentAsync(companyId, employeeId, documentId);
        if (!deleted)
            return NotFoundError("Document not found.");

        return NoContent();
    }
}
