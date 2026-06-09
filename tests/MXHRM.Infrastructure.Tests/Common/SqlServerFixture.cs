using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MXHRM.Infrastructure.Data;

namespace MXHRM.Infrastructure.Tests.Common;

// Fixture = ของแพงที่ setup ครั้งเดียว แล้วแชร์ให้ทุก test ใน class (ผ่าน IClassFixture)
public sealed class SqlServerFixture : IAsyncLifetime
{
    // DB ชื่อไม่ซ้ำต่อ run → test แต่ละครั้งแยกกัน ไม่ชนกัน
    private readonly string _dbName = $"MXHRM_Test_{Guid.NewGuid():N}";

    private const string MasterConnection =
        "Server=127.0.0.1,1433;Database=master;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True;";

    public string ConnectionString { get; private set; } = string.Empty;

    // factory: เปิด AppDbContext ใหม่ที่ชี้ DB ทดสอบ (จำลอง "request ใหม่/user ใหม่")
    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        // tenantProvider = null → bypass query filter (เห็นทุกบริษัท)
        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        ConnectionString =
            $"Server=127.0.0.1,1433;Database={_dbName};User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True;";

        // EnsureCreatedAsync: ถ้า DB ยังไม่มี → สร้าง DB + schema จาก model เลย (พอสำหรับ test)
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        // ลบ DB ทิ้งหลัง test เสร็จ → ไม่ทิ้งขยะค้างใน SQL Server
        await using var connection = new SqlConnection(MasterConnection);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [{_dbName}];";
        await command.ExecuteNonQueryAsync();
    }
}