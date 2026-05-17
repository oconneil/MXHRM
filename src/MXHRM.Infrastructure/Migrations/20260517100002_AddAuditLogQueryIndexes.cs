using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MXHRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogQueryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_Action_CreatedAtUtc",
                table: "AuditLogs",
                columns: new[] { "Action", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableName_CreatedAtUtc",
                table: "AuditLogs",
                columns: new[] { "TableName", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAtUtc",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_Action_CreatedAtUtc",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_TableName_CreatedAtUtc",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId_CreatedAtUtc",
                table: "AuditLogs");
        }
    }
}
