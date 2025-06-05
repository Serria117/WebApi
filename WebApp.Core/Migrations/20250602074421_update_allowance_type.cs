using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class update_allowance_type : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "PR_AllowanceRate");

            migrationBuilder.AddColumn<int>(
                name: "AllowanceTypeId",
                table: "PR_AllowanceRate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PR_AllowanceTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsInsurance = table.Column<bool>(type: "bit", nullable: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PR_AllowanceTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PR_AllowanceRate_AllowanceTypeId",
                table: "PR_AllowanceRate",
                column: "AllowanceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PR_AllowanceRate_PR_AllowanceTypes_AllowanceTypeId",
                table: "PR_AllowanceRate",
                column: "AllowanceTypeId",
                principalTable: "PR_AllowanceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PR_AllowanceRate_PR_AllowanceTypes_AllowanceTypeId",
                table: "PR_AllowanceRate");

            migrationBuilder.DropTable(
                name: "PR_AllowanceTypes");

            migrationBuilder.DropIndex(
                name: "IX_PR_AllowanceRate_AllowanceTypeId",
                table: "PR_AllowanceRate");

            migrationBuilder.DropColumn(
                name: "AllowanceTypeId",
                table: "PR_AllowanceRate");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PR_AllowanceRate",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
