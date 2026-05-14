using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MXHRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Employees_CompanyID_IsActive_EmployeeID",
                table: "Employees",
                columns: new[] { "CompanyID", "IsActive", "EmployeeID" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeID",
                table: "Employees",
                column: "EmployeeID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_CompanyID_IsActive_EmployeeID",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Email",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeID",
                table: "Employees");
        }
    }
}
