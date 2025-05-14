using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_user_log : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TaxOffices_ParentId",
                table: "TaxOffices",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaxOffices_TaxOffices_ParentId",
                table: "TaxOffices",
                column: "ParentId",
                principalTable: "TaxOffices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaxOffices_TaxOffices_ParentId",
                table: "TaxOffices");

            migrationBuilder.DropIndex(
                name: "IX_TaxOffices_ParentId",
                table: "TaxOffices");
        }
    }
}
