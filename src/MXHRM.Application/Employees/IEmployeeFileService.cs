using MXHRM.Application.Common;
using MXHRM.Application.Employees.DTOs;

namespace MXHRM.Application.Employees;

public interface IEmployeeFileService
{
    // คืน storage key ของรูปใหม่ / null ถ้าไม่เจอพนักงาน
    Task<string?> SetPhotoAsync(string companyId, string employeeId, Stream content, string fileName, CancellationToken ct = default);

    // คืนไฟล์รูป / null ถ้าไม่มีพนักงานหรือไม่มีรูป
    Task<FileDownloadResult?> GetPhotoAsync(string companyId, string employeeId, CancellationToken ct = default);

    // true ถ้าลบสำเร็จ / false ถ้าไม่เจอพนักงาน
    Task<bool> DeletePhotoAsync(string companyId, string employeeId, CancellationToken ct = default);

    // ===== Documents (หลายไฟล์ต่อพนักงาน) =====
    Task<EmployeeDocumentResponse?> AddDocumentAsync(string companyId, string employeeId, Stream content, string fileName, string contentType, long size, string documentType, CancellationToken ct = default);
    Task<IReadOnlyList<EmployeeDocumentResponse>> ListDocumentsAsync(string companyId, string employeeId, CancellationToken ct = default);
    Task<FileDownloadResult?> GetDocumentAsync(string companyId, string employeeId, Guid documentId, CancellationToken ct = default);
    Task<bool> DeleteDocumentAsync(string companyId, string employeeId, Guid documentId, CancellationToken ct = default);
}