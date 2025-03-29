using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_index_bl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ImportedBalanceSheets",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportedBalanceSheets_CreateAt",
                table: "ImportedBalanceSheets",
                column: "CreateAt");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedBalanceSheetDetails_Account",
                table: "ImportedBalanceSheetDetails",
                column: "Account");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportedBalanceSheets_CreateAt",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropIndex(
                name: "IX_ImportedBalanceSheetDetails_Account",
                table: "ImportedBalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ImportedBalanceSheets");
        }
    }
}
