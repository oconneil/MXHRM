using Microsoft.EntityFrameworkCore;
using MXHRM.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MXHRM.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

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
    }
}