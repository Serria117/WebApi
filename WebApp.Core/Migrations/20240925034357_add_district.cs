using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_district : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Organizations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_DistrictId",
                table: "Organizations",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Organizations_RegionDistrict_DistrictId",
                table: "Organizations",
                column: "DistrictId",
                principalTable: "RegionDistrict",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organizations_RegionDistrict_DistrictId",
                table: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_Organizations_DistrictId",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Organizations");
        }
    }
}
