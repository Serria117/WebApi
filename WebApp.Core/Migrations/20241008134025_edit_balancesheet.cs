using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class edit_balancesheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BalanceSheets_Organizations_OrganizationId",
                table: "BalanceSheets");

            migrationBuilder.DropForeignKey(
                name: "FK_RegionDistrict_RegionProvince_ProvinceId",
                table: "RegionDistrict");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxOffices_RegionProvince_ProvinceId",
                table: "TaxOffices");

            migrationBuilder.DropIndex(
                name: "IX_ImportedBalanceSheets_BalanceSheetId",
                table: "ImportedBalanceSheets");

            migrationBuilder.AlterColumn<int>(
                name: "ProvinceId",
                table: "TaxOffices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "ProvinceId",
                table: "RegionDistrict",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "BalanceSheetId",
                table: "ImportedBalanceSheets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "ImportedBalanceSheets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "ImportedBalanceSheets",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsValid",
                table: "ImportedBalanceSheetDetails",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                table: "BalanceSheets",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<int>(
                name: "BlId",
                table: "BalanceSheetDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedBalanceSheets_BalanceSheetId",
                table: "ImportedBalanceSheets",
                column: "BalanceSheetId",
                unique: true,
                filter: "[BalanceSheetId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedBalanceSheets_OrganizationId",
                table: "ImportedBalanceSheets",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BalanceSheets_Organizations_OrganizationId",
                table: "BalanceSheets",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportedBalanceSheets_Organizations_OrganizationId",
                table: "ImportedBalanceSheets",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegionDistrict_RegionProvince_ProvinceId",
                table: "RegionDistrict",
                column: "ProvinceId",
                principalTable: "RegionProvince",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxOffices_RegionProvince_ProvinceId",
                table: "TaxOffices",
                column: "ProvinceId",
                principalTable: "RegionProvince",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BalanceSheets_Organizations_OrganizationId",
                table: "BalanceSheets");

            migrationBuilder.DropForeignKey(
                name: "FK_ImportedBalanceSheets_Organizations_OrganizationId",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropForeignKey(
                name: "FK_RegionDistrict_RegionProvince_ProvinceId",
                table: "RegionDistrict");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxOffices_RegionProvince_ProvinceId",
                table: "TaxOffices");

            migrationBuilder.DropIndex(
                name: "IX_ImportedBalanceSheets_BalanceSheetId",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropIndex(
                name: "IX_ImportedBalanceSheets_OrganizationId",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "ImportedBalanceSheets");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "ImportedBalanceSheets");

            migrationBuilder.AlterColumn<int>(
                name: "ProvinceId",
                table: "TaxOffices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProvinceId",
                table: "RegionDistrict",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BalanceSheetId",
                table: "ImportedBalanceSheets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsValid",
                table: "ImportedBalanceSheetDetails",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                table: "BalanceSheets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BlId",
                table: "BalanceSheetDetails",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportedBalanceSheets_BalanceSheetId",
                table: "ImportedBalanceSheets",
                column: "BalanceSheetId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BalanceSheets_Organizations_OrganizationId",
                table: "BalanceSheets",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegionDistrict_RegionProvince_ProvinceId",
                table: "RegionDistrict",
                column: "ProvinceId",
                principalTable: "RegionProvince",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxOffices_RegionProvince_ProvinceId",
                table: "TaxOffices",
                column: "ProvinceId",
                principalTable: "RegionProvince",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
