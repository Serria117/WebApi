using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class update_date_document : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DocumentDate",
                table: "Documents",
                type: "datetime2",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdjustmentType",
                table: "Documents",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfAdjustment",
                table: "Documents",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PeriodType",
                table: "Documents",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdjustmentType",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "NumberOfAdjustment",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PeriodType",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "DocumentDate",
                table: "Documents",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldMaxLength: 10,
                oldNullable: true);
        }
    }
}
