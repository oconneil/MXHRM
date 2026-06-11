namespace MXHRM.Application.Common;

// ผลลัพธ์ตอนขอไฟล์: stream + ชนิด + ชื่อ (controller เอาไปทำ File())
public sealed record FileDownloadResult(Stream Content, string ContentType, string FileName);