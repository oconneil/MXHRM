using MXHRM.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MXHRM.Domain.Employees;

// เอกสารของพนักงาน (1 พนักงาน : หลายไฟล์) — derive BaseEntity → ได้ CompanyID/audit/RowVersion + tenant filter
public class EmployeeDocument : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(20)]
    public string EmployeeID { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;      // ชื่อไฟล์เดิมของผู้ใช้ (ไว้โชว์/ดาวน์โหลด)

    [Required, MaxLength(400)]
    public string StoragePath { get; set; } = string.Empty;   // storage key (ของจริงในดิสก์)

    [Required, MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;  // เช่น "Contract", "IDCard", "Resume"
}