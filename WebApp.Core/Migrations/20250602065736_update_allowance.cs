using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class update_allowance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PR_PayrollComponentType_PR_PayrollInputType_InputTypeId",
                table: "PR_PayrollComponentType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AllowanceRates",
                table: "AllowanceRates");

            migrationBuilder.RenameTable(
                name: "AllowanceRates",
                newName: "PR_AllowanceRate");

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "PR_AllowanceRate",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PR_AllowanceRate",
                table: "PR_AllowanceRate",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PR_PayrollComponentType_PR_PayrollInputType_InputTypeId",
                table: "PR_PayrollComponentType",
                column: "InputTypeId",
                principalTable: "PR_PayrollInputType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PR_PayrollComponentType_PR_PayrollInputType_InputTypeId",
                table: "PR_PayrollComponentType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PR_AllowanceRate",
                table: "PR_AllowanceRate");

            migrationBuilder.RenameTable(
                name: "PR_AllowanceRate",
                newName: "AllowanceRates");

            migrationBuilder.AlterColumn<string>(
                name: "JobTitle",
                table: "AllowanceRates",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AllowanceRates",
                table: "AllowanceRates",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PR_PayrollComponentType_PR_PayrollInputType_InputTypeId",
                table: "PR_PayrollComponentType",
                column: "InputTypeId",
                principalTable: "PR_PayrollInputType",
                principalColumn: "Id");
        }
    }
}
