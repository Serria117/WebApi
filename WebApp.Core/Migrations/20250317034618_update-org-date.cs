using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class updateorgdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FiscalYearFistDate",
                table: "Organizations",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BalanceSheetEntry",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BalanceSheetId = table.Column<int>(type: "int", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    OpenCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OpenDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AriseCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AriseDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CloseCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CloseDebit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceSheetEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceSheetEntry_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BalanceSheetEntry_BalanceSheets_BalanceSheetId",
                        column: x => x.BalanceSheetId,
                        principalTable: "BalanceSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalanceSheetEntry_AccountId",
                table: "BalanceSheetEntry",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BalanceSheetEntry_BalanceSheetId",
                table: "BalanceSheetEntry",
                column: "BalanceSheetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceSheetEntry");

            migrationBuilder.DropColumn(
                name: "FiscalYearFistDate",
                table: "Organizations");
        }
    }
}
