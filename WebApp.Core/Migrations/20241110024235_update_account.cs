using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class update_account : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "B01NV",
                table: "BalanceSheetDetails",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B01TS",
                table: "BalanceSheetDetails",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B02",
                table: "BalanceSheetDetails",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Parent",
                table: "BalanceSheetDetails",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B01NV",
                table: "Accounts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B01TS",
                table: "Accounts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "B02",
                table: "Accounts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SyncInvoiceHistories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TaxId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SyncType = table.Column<int>(type: "int", nullable: false),
                    TotalFound = table.Column<int>(type: "int", nullable: false),
                    TotalSuccess = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyncInvoiceHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyncInvoiceHistories_TaxId",
                table: "SyncInvoiceHistories",
                column: "TaxId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyncInvoiceHistories");

            migrationBuilder.DropColumn(
                name: "B01NV",
                table: "BalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "B01TS",
                table: "BalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "B02",
                table: "BalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "Parent",
                table: "BalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "B01NV",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "B01TS",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "B02",
                table: "Accounts");
        }
    }
}
