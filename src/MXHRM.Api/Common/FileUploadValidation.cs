namespace MXHRM.Api.Common;

public static class FileUploadValidation
{
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxImageBytes = 2 * 1024 * 1024; // 2 MB

    // คืน error message ถ้าผิด / null ถ้าผ่าน
    public static string? ValidateImage(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return "No file uploaded.";

        if (file.Length > MaxImageBytes)
            return "Image must not exceed 2 MB.";

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(ext))
            return "Only .jpg, .jpeg, .png, .webp images are allowed.";

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return "File is not a valid image.";

        return null;
    }

    private static readonly string[] AllowedDocExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".docx", ".xlsx"];
    private const long MaxDocBytes = 10 * 1024 * 1024; // 10 MB

    public static string? ValidateDocument(IFormFile? file)
    {
        if (file is null || file.Length == 0)
            return "No file uploaded.";

        if (file.Length > MaxDocBytes)
            return "Document must not exceed 10 MB.";

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedDocExtensions.Contains(ext))
            return "Allowed types: pdf, jpg, jpeg, png, docx, xlsx.";

        return null;
    }
}