using System.ComponentModel.DataAnnotations;

namespace MXHRM.Domain.Common;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
        CompanyID = string.Empty;
        CreatedBy = string.Empty;
        ModifiedBy = string.Empty;
        CreatedDate = DateTime.UtcNow;
        ModifiedDate = DateTime.UtcNow;
    }

    [Key]
    [Required(ErrorMessage = "CompanyID is required.")]
    [MaxLength(20)]
    public string CompanyID { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = [];

    public DateTime CreatedDate { get; set; }

    [MaxLength(100)]
    public string CreatedBy { get; set; }

    public DateTime ModifiedDate { get; set; }

    [MaxLength(100)]
    public string ModifiedBy { get; set; }
}
