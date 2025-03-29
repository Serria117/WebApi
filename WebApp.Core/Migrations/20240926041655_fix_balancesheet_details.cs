using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class fix_balancesheet_details : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecordTime",
                table: "BalanceSheetDetails");

            migrationBuilder.RenameColumn(
                name: "DebitValue",
                table: "ImportedBalanceSheetDetails",
                newName: "OpenDebit");

            migrationBuilder.RenameColumn(
                name: "CreditValue",
                table: "ImportedBalanceSheetDetails",
                newName: "OpenCredit");

            migrationBuilder.RenameColumn(
                name: "DebitValue",
                table: "BalanceSheetDetails",
                newName: "OpenDebit");

            migrationBuilder.RenameColumn(
                name: "CreditValue",
                table: "BalanceSheetDetails",
                newName: "OpenCredit");

            migrationBuilder.AddColumn<decimal>(
                name: "AriseCredit",
                table: "ImportedBalanceSheetDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AriseDebit",
                table: "ImportedBalanceSheetDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CloseCredit",
                table: "ImportedBalanceSheetDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CloseDebit",
                table: "ImportedBalanceSheetDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AriseCredit",
                table: "BalanceSheetDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "AriseDebit",
                table: "BalanceSheetDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CloseCredit",
                table: "BalanceSheetDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CloseDebit",
                table: "BalanceSheetDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AriseCredit",
                table: "ImportedBalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "AriseDebit",
                table: "ImportedBalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "CloseCredit",
                table: "ImportedBalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "CloseDebit",
                table: "ImportedBalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "AriseCredit",
                table: "BalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "AriseDebit",
                table: "BalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "CloseCredit",
                table: "BalanceSheetDetails");

            migrationBuilder.DropColumn(
                name: "CloseDebit",
                table: "BalanceSheetDetails");

            migrationBuilder.RenameColumn(
                name: "OpenDebit",
                table: "ImportedBalanceSheetDetails",
                newName: "DebitValue");

            migrationBuilder.RenameColumn(
                name: "OpenCredit",
                table: "ImportedBalanceSheetDetails",
                newName: "CreditValue");

            migrationBuilder.RenameColumn(
                name: "OpenDebit",
                table: "BalanceSheetDetails",
                newName: "DebitValue");

            migrationBuilder.RenameColumn(
                name: "OpenCredit",
                table: "BalanceSheetDetails",
                newName: "CreditValue");

            migrationBuilder.AddColumn<int>(
                name: "RecordTime",
                table: "BalanceSheetDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
