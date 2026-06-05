using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MXHRM.Domain.Employees;
using MXHRM.Infrastructure.Auth;
using MXHRM.Infrastructure.Authorization;
using MXHRM.Infrastructure.Identity;
using MXHRM.Infrastructure.Auditing;
using MXHRM.Domain.Reports;
using MXHRM.Domain.Notifications;
using MXHRM.Domain.Common;
using MXHRM.Application.Common.Interfaces;
using System.Reflection;

namespace MXHRM.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly string? _tenantCompanyId;
    private readonly bool _bypassTenant;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ITenantProvider? tenantProvider = null)
        : base(options)
    {
        _tenantCompanyId = tenantProvider?.CompanyId;
        // ไม่มี provider (test / seeder / design-time) → bypass = เห็นทุกบริษัท
        _bypassTenant = tenantProvider?.BypassTenantFilter ?? true;
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();
    public DbSet<GeneratedReport> GeneratedReports => Set<GeneratedReport>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");

            entity.HasKey(e => new
            {
                e.CompanyID,
                e.EmployeeID
            });

            entity.Property(e => e.RowVersion)
                .IsRowVersion();

            entity.Property(e => e.Salary)
                .HasColumnType("decimal(18,2)");

            entity.HasIndex(e => e.EmployeeID)
                .HasDatabaseName("IX_Employees_EmployeeID");

            entity.HasIndex(e => e.Email)
                .HasDatabaseName("IX_Employees_Email");

            entity.HasIndex(e => new
            {
                e.CompanyID,
                e.IsActive,
                e.EmployeeID
            })
                .HasDatabaseName("IX_Employees_CompanyID_IsActive_EmployeeID");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(x => x.Token)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasIndex(x => x.Token)
                .IsUnique();

            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(x => new { x.RoleId, x.PermissionId });

            entity.HasOne(x => x.Role)
                .WithMany()
                .HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Permission)
                .WithMany(x => x.RolePermissions)
                .HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TableName)
                .HasMaxLength(128)
                .IsRequired();

            entity.Property(x => x.Action)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.UserId)
                .HasMaxLength(450);

            entity.Property(x => x.UserName)
                .HasMaxLength(256);

            entity.Property(x => x.TraceId)
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => x.TableName)
                .HasDatabaseName("IX_AuditLogs_TableName");

            entity.HasIndex(x => x.CreatedAtUtc)
                .HasDatabaseName("IX_AuditLogs_CreatedAtUtc");

            entity.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_AuditLogs_UserId");

            entity.HasIndex(x => new
            {
                x.TableName,
                x.CreatedAtUtc
            })
                .HasDatabaseName("IX_AuditLogs_TableName_CreatedAtUtc");

            entity.HasIndex(x => new
            {
                x.UserId,
                x.CreatedAtUtc
            })
                .HasDatabaseName("IX_AuditLogs_UserId_CreatedAtUtc");

            entity.HasIndex(x => new
            {
                x.Action,
                x.CreatedAtUtc
            })
                .HasDatabaseName("IX_AuditLogs_Action_CreatedAtUtc");
        });

        modelBuilder.Entity<UserActivityLog>(entity =>
        {
            entity.ToTable("UserActivityLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ActivityType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.UserId)
                .HasMaxLength(450);

            entity.Property(x => x.UserName)
                .HasMaxLength(256);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(100);

            entity.Property(x => x.UserAgent)
                .HasMaxLength(500);

            entity.Property(x => x.TraceId)
                .HasMaxLength(100);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => x.ActivityType)
                .HasDatabaseName("IX_UserActivityLogs_ActivityType");

            entity.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_UserActivityLogs_UserId");

            entity.HasIndex(x => x.CreatedAtUtc)
                .HasDatabaseName("IX_UserActivityLogs_CreatedAtUtc");

            entity.HasIndex(x => new
            {
                x.UserId,
                x.CreatedAtUtc
            })
                .HasDatabaseName("IX_UserActivityLogs_UserId_CreatedAtUtc");

            entity.HasIndex(x => new
            {
                x.ActivityType,
                x.CreatedAtUtc
            })
                .HasDatabaseName("IX_UserActivityLogs_ActivityType_CreatedAtUtc");
        });

        modelBuilder.Entity<GeneratedReport>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReportType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Format)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.FileName)
                .HasMaxLength(255);

            entity.Property(x => x.ContentType)
                .HasMaxLength(150);

            entity.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            entity.Property(x => x.RequestedByUserId)
                .HasMaxLength(450);

            entity.Property(x => x.RequestedByUserName)
                .HasMaxLength(256);

            entity.Property(x => x.Content)
                .HasColumnType("varbinary(max)");

            entity.HasIndex(x => x.Status);

            entity.HasIndex(x => x.ReportType);

            entity.HasIndex(x => x.CreatedAtUtc);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("UserNotifications");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId)
                .HasMaxLength(450)
                .IsRequired();

            entity.Property(x => x.Type)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Key)
                .HasMaxLength(200);

            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Message)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.Tone)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.DataJson)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.Route)
                .HasMaxLength(300);

            entity.Property(x => x.CreatedAtUtc)
                .IsRequired();

            entity.Property(x => x.UpdatedAtUtc)
                .IsRequired();

            entity.HasIndex(x => new { x.UserId, x.UpdatedAtUtc })
                .HasDatabaseName("IX_UserNotifications_UserId_UpdatedAtUtc");

            entity.HasIndex(x => new { x.UserId, x.IsRead, x.UpdatedAtUtc })
                .HasDatabaseName("IX_UserNotifications_UserId_IsRead_UpdatedAtUtc");

            entity.HasIndex(x => new { x.UserId, x.Key })
                .IsUnique()
                .HasFilter("[Key] IS NOT NULL")
                .HasDatabaseName("UX_UserNotifications_UserId_Key");
        });

        // === Multi-tenant: ใส่ global query filter (auto WHERE CompanyID) ===
        // ครอบคลุมทุก entity ที่ derive จาก BaseEntity (ตอนนี้ = Employee, อนาคตเพิ่มก็ครอบเอง)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                typeof(AppDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), BindingFlags.Instance | BindingFlags.NonPublic)!
                    .MakeGenericMethod(entityType.ClrType)
                    .Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    // generic helper: ทำให้ lambda เป็น strongly-typed (e.CompanyID เข้าถึงได้)
    // _bypassTenant = true → filter ผ่านหมด (เห็นทุกบริษัท) / false → เฉพาะ tenant
    private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder)
        where TEntity : BaseEntity
    {
        modelBuilder.Entity<TEntity>()
            .HasQueryFilter(e => _bypassTenant || e.CompanyID == _tenantCompanyId);
    }
}
