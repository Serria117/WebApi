using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_taxOffice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaxOfficeId",
                table: "Organizations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganizationInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    District = table.Column<int>(type: "int", nullable: false),
                    TaxOfficeId = table.Column<int>(type: "int", nullable: false),
                    Organization = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationInfos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_TaxOfficeId",
                table: "Organizations",
                column: "TaxOfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_TaxOffices_TaxOfficeId",
                table: "Organizations",
                column: "TaxOfficeId",
                principalTable: "TaxOffices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_TaxOffices_TaxOfficeId",
                table: "Organizations");

            migrationBuilder.DropTable(
                name: "OrganizationInfos");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_TaxOfficeId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "TaxOfficeId",
                table: "Organizations");
        }
    }
}
