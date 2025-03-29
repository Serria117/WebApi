using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_risk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "ImportedBalanceSheetDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "ImportedBalanceSheetDetails",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RiskCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    TaxOffice = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskCompanies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskCompanies_TaxId",
                table: "RiskCompanies",
                column: "TaxId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskCompanies");

            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "ImportedBalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "ImportedBalanceSheetDetails");
        }
    }
}
