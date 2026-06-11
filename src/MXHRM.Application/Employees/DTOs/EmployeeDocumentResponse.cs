namespace MXHRM.Application.Employees.DTOs;

public sealed record EmployeeDocumentResponse(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes,
    string DocumentType,
    DateTime UploadedAt,
    string UploadedBy);