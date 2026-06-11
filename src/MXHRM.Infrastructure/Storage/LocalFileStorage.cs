using Microsoft.Extensions.Configuration;
using MXHRM.Application.Common.Interfaces;

namespace MXHRM.Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IConfiguration configuration)
    {
        // โฟลเดอร์รากของไฟล์ทั้งหมด (config ได้ เผื่อ deploy คนละ path)
        _root = Path.GetFullPath(configuration["FileStorage:RootPath"] ?? "App_Data/uploads");
    }

    public async Task<string> SaveAsync(Stream content, string subFolder, string originalFileName, CancellationToken ct = default)
    {
        // ❶ ตั้งชื่อใหม่ด้วย Guid → กันชื่อชนกัน + กันชื่อไฟล์มุ่งร้ายจากผู้ใช้
        var extension = Path.GetExtension(originalFileName);
        var safeName = $"{Guid.NewGuid():N}{extension}";

        var folderPath = Path.Combine(_root, subFolder);
        Directory.CreateDirectory(folderPath);

        var fullPath = Path.Combine(folderPath, safeName);

        await using var fileStream = File.Create(fullPath);
        await content.CopyToAsync(fileStream, ct);

        // คืน key สัมพัทธ์ (ใช้ '/' เสมอเพื่อ cross-platform)
        return $"{subFolder}/{safeName}";
    }

    public Task<Stream?> OpenReadAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = ResolveSafePath(storageKey);

        if (fullPath is null || !File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = ResolveSafePath(storageKey);

        if (fullPath is not null && File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    // ❷ กัน path traversal: resolve แล้วต้องอยู่ "ใต้ _root" เท่านั้น
    private string? ResolveSafePath(string storageKey)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_root, storageKey));

        var rootWithSep = _root.EndsWith(Path.DirectorySeparatorChar)
            ? _root
            : _root + Path.DirectorySeparatorChar;

        return fullPath.StartsWith(rootWithSep, StringComparison.Ordinal)
            ? fullPath
            : null;   // พยายามออกนอก root (เช่น ../../) → ปฏิเสธ
    }
}