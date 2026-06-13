namespace MXHRM.Api.Common;

public class FileUploadOptions
{
    public const string SectionName = "FileUpload";

    public FileUploadRule Photo { get; set; } = new();
    public FileUploadRule Document { get; set; } = new();
}

public class FileUploadRule
{
    public long MaxBytes { get; set; }
    public string[] AllowedExtensions { get; set; } = [];
}