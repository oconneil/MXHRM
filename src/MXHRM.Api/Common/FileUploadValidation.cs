namespace MXHRM.Api.Common;

public static class FileUploadValidation
{
    // ไม่มี magic number แล้ว — รับ rule จาก config มาเช็ค
    public static string? Validate(IFormFile? file, FileUploadRule rule)
    {
        if (file is null || file.Length == 0)
            return "No file uploaded.";

        if (file.Length > rule.MaxBytes)
            return $"File must not exceed {rule.MaxBytes / (1024 * 1024)} MB.";

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!rule.AllowedExtensions.Contains(ext))
            return $"Allowed types: {string.Join(", ", rule.AllowedExtensions)}.";

        return null;
    }
}