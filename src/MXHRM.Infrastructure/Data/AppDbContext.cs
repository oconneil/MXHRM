using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MXHRM.Domain.Employees;
using MXHRM.Infrastructure.Auth;
using MXHRM.Infrastructure.Authorization;
using MXHRM.Infrastructure.Identity;
using MXHRM.Infrastructure.Auditing;

namespace MXHRM.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();


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
        });

    }
}
