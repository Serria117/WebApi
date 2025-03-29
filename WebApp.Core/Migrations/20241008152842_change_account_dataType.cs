using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class change_account_dataType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "ImportedBalanceSheets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SumAriseCredit",
                table: "ImportedBalanceSheets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SumAriseDebit",
                table: "ImportedBalanceSheets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SumCloseCredit",
                table: "ImportedBalanceSheets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SumCloseDebit",
                table: "ImportedBalanceSheets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SumOpenCredit",
                table: "ImportedBalanceSheets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SumOpenDebit",
                table: "ImportedBalanceSheets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                table: "ImportedBalanceSheetDetails",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Account",
                table: "BalanceSheetDetails",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "Accounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "SumAriseCredit",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "SumAriseDebit",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "SumCloseCredit",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "SumCloseDebit",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "SumOpenCredit",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "SumOpenDebit",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "Accounts");

            migrationBuilder.AlterColumn<int>(
                name: "Account",
                table: "ImportedBalanceSheetDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Account",
                table: "BalanceSheetDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);
        }
    }
}
