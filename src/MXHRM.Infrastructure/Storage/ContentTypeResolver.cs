namespace MXHRM.Infrastructure.Storage;

public static class ContentTypeResolver
{
    // map นามสกุล → MIME type (ถ้าไม่รู้จัก = octet-stream ให้ browser ดาวน์โหลด)
    public static string FromPath(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();

        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };
    }
}