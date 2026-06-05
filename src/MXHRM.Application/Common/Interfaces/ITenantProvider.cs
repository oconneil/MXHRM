namespace MXHRM.Application.Common.Interfaces;

public interface ITenantProvider
{
    string? CompanyId { get; }          // บริษัทของ request นี้ (จาก JWT)
    bool BypassTenantFilter { get; }    // true = ไม่กรอง (admin / job / ไม่ได้ login)
}