using MXHRM.Api.DTOs.Employees;
using MXHRM.Api.Models;
using MXHRM.Api.DTOs.Common;

namespace MXHRM.Api.Services.Employees;

public interface IEmployeeService
{
    Task<PagedResponse<EmployeeResponse>> GetAllAsync(GetEmployeesRequest request);
    Task<EmployeeResponse?> GetByIdAsync(string companyId, string employeeId);
    Task<EmployeeResponse> CreateAsync(CreateEmployeeRequest request);
    Task<bool> UpdateAsync(string companyId, string employeeId, UpdateEmployeeRequest request);
    Task<bool> DeleteAsync(string companyId, string employeeId);
}