using Microsoft.EntityFrameworkCore;
using MXHRM.Application.Common;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Application.Employees;
using MXHRM.Infrastructure.Data;
using MXHRM.Infrastructure.Storage;
using MXHRM.Application.Employees.DTOs;
using MXHRM.Domain.Employees;

namespace MXHRM.Infrastructure.Employees;

public class EmployeeFileService(
    AppDbContext db,
    IFileStorage storage,
    ICacheService cache) : IEmployeeFileService
{
    private const string PhotoFolder = "employee-photos";
    private const string DocumentFolder = "employee-documents";

    public async Task<string?> SetPhotoAsync(string companyId, string employeeId, Stream content, string fileName, CancellationToken ct = default)
    {
        var employee = await db.Employees
            .FirstOrDefaultAsync(x => x.CompanyID == companyId && x.EmployeeID == employeeId, ct);

        if (employee is null)
            return null;

        // ลบรูปเก่า กันไฟล์ขยะค้างใน storage
        if (!string.IsNullOrWhiteSpace(employee.PhotoPath))
            await storage.DeleteAsync(employee.PhotoPath, ct);

        // แยกโฟลเดอร์ตาม tenant
        var key = await storage.SaveAsync(content, $"{PhotoFolder}/{companyId}", fileName, ct);
        employee.PhotoPath = key;

        await db.SaveChangesAsync(ct);

        // ล้าง cache employee เพราะ PhotoPath เปลี่ยน
        await cache.RemoveByPrefixAsync("employees:", ct);

        return key;
    }

    public async Task<FileDownloadResult?> GetPhotoAsync(string companyId, string employeeId, CancellationToken ct = default)
    {
        var photoPath = await db.Employees
            .AsNoTracking()
            .Where(x => x.CompanyID == companyId && x.EmployeeID == employeeId)
            .Select(x => x.PhotoPath)
            .FirstOrDefaultAsync(ct);

        if (string.IsNullOrWhiteSpace(photoPath))
            return null;

        var stream = await storage.OpenReadAsync(photoPath, ct);
        if (stream is null)
            return null;

        return new FileDownloadResult(stream, ContentTypeResolver.FromPath(photoPath), Path.GetFileName(photoPath));
    }

    public async Task<bool> DeletePhotoAsync(string companyId, string employeeId, CancellationToken ct = default)
    {
        var employee = await db.Employees
            .FirstOrDefaultAsync(x => x.CompanyID == companyId && x.EmployeeID == employeeId, ct);

        if (employee is null)
            return false;

        if (!string.IsNullOrWhiteSpace(employee.PhotoPath))
            await storage.DeleteAsync(employee.PhotoPath, ct);

        employee.PhotoPath = null;
        await db.SaveChangesAsync(ct);
        await cache.RemoveByPrefixAsync("employees:", ct);

        return true;
    }

    public async Task<EmployeeDocumentResponse?> AddDocumentAsync(string companyId, string employeeId, Stream content, string fileName, string contentType, long size, string documentType, CancellationToken ct = default)
    {
        var exists = await db.Employees.AsNoTracking()
            .AnyAsync(x => x.CompanyID == companyId && x.EmployeeID == employeeId, ct);
        if (!exists)
            return null;

        var key = await storage.SaveAsync(content, $"{DocumentFolder}/{companyId}/{employeeId}", fileName, ct);

        var doc = new EmployeeDocument
        {
            CompanyID = companyId,
            EmployeeID = employeeId,
            FileName = fileName,
            StoragePath = key,
            ContentType = contentType,
            SizeBytes = size,
            DocumentType = documentType
        };

        db.EmployeeDocuments.Add(doc);
        await db.SaveChangesAsync(ct);

        return ToDocResponse(doc);
    }

    public async Task<IReadOnlyList<EmployeeDocumentResponse>> ListDocumentsAsync(string companyId, string employeeId, CancellationToken ct = default)
    {
        return await db.EmployeeDocuments
            .AsNoTracking()
            .Where(x => x.CompanyID == companyId && x.EmployeeID == employeeId)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new EmployeeDocumentResponse(
                x.Id, x.FileName, x.ContentType, x.SizeBytes, x.DocumentType, x.CreatedDate, x.CreatedBy))
            .ToListAsync(ct);
    }

    public async Task<FileDownloadResult?> GetDocumentAsync(string companyId, string employeeId, Guid documentId, CancellationToken ct = default)
    {
        var doc = await db.EmployeeDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentId && x.CompanyID == companyId && x.EmployeeID == employeeId, ct);

        if (doc is null)
            return null;

        var stream = await storage.OpenReadAsync(doc.StoragePath, ct);
        if (stream is null)
            return null;

        return new FileDownloadResult(stream, doc.ContentType, doc.FileName);
    }

    public async Task<bool> DeleteDocumentAsync(string companyId, string employeeId, Guid documentId, CancellationToken ct = default)
    {
        var doc = await db.EmployeeDocuments
            .FirstOrDefaultAsync(x => x.Id == documentId && x.CompanyID == companyId && x.EmployeeID == employeeId, ct);

        if (doc is null)
            return false;

        await storage.DeleteAsync(doc.StoragePath, ct);
        db.EmployeeDocuments.Remove(doc);
        await db.SaveChangesAsync(ct);

        return true;
    }

    private static EmployeeDocumentResponse ToDocResponse(EmployeeDocument d) =>
        new(d.Id, d.FileName, d.ContentType, d.SizeBytes, d.DocumentType, d.CreatedDate, d.CreatedBy);
}