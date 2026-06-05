using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MXHRM.Application.Common.Interfaces;
using MXHRM.Domain.Common;

namespace MXHRM.Infrastructure.Data;

public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantProvider _tenant;

    public AuditSaveChangesInterceptor(ICurrentUserService currentUser, ITenantProvider tenant)
    {
        _currentUser = currentUser;
        _tenant = tenant;
    }

    // ดัก async path (ที่เราใช้ SaveChangesAsync)
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    // ดัก sync path ด้วย (เผื่อมีที่ไหนเรียก SaveChanges ปกติ)
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context is null) return;

        // ใครทำ — จาก JWT (job/seeder ไม่มี user → "system")
        var user = _currentUser.UserName ?? _currentUser.UserId ?? "system";
        var now = DateTime.UtcNow;

        // วนเฉพาะ entity ที่ derive BaseEntity และกำลังถูก Add/Modify
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // auto-stamp tenant CompanyID ถ้ายังว่าง (เช่น สร้าง entity โดยไม่ระบุบริษัท)
                // job/seeder (tenant=null) จะไม่ถูกแตะ → seeder set CompanyID เองได้ปกติ
                if (string.IsNullOrEmpty(entry.Entity.CompanyID) &&
                    !string.IsNullOrEmpty(_tenant.CompanyId))
                {
                    entry.Entity.CompanyID = _tenant.CompanyId;
                }

                entry.Entity.CreatedBy = user;
                entry.Entity.CreatedDate = now;
                entry.Entity.ModifiedBy = user;
                entry.Entity.ModifiedDate = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedBy = user;
                entry.Entity.ModifiedDate = now;

                // กัน CreatedBy/CreatedDate ถูกเขียนทับตอน update
                entry.Property(e => e.CreatedBy).IsModified = false;
                entry.Property(e => e.CreatedDate).IsModified = false;
            }
        }
    }
}