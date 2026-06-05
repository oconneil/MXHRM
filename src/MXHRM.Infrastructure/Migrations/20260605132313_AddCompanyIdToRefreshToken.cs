using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MXHRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyIdToRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyID",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "RefreshTokens");
        }
    }
}
