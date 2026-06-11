namespace MXHRM.Application.Common.Interfaces;

public interface IFileStorage
{
    // เก็บไฟล์ → คืน "storage key" (path สัมพัทธ์) ไว้บันทึกลง DB
    Task<string> SaveAsync(Stream content, string subFolder, string originalFileName, CancellationToken ct = default);

    // เปิดอ่านไฟล์จาก key → null ถ้าไม่เจอ
    Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct = default);

    // ลบไฟล์ (เงียบถ้าไม่มี)
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}