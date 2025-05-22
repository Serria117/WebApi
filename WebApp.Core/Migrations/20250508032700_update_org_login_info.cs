using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class update_org_login_info : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLoginInfos_Organizations_OrganizationId",
                table: "OrganizationLoginInfos");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                table: "OrganizationLoginInfos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationLoginInfos_Organizations_OrganizationId",
                table: "OrganizationLoginInfos",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationLoginInfos_Organizations_OrganizationId",
                table: "OrganizationLoginInfos");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrganizationId",
                table: "OrganizationLoginInfos",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationLoginInfos_Organizations_OrganizationId",
                table: "OrganizationLoginInfos",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }
    }
}
