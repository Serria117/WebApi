using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class update_emp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseSalary",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceSalary",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseSalary",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "InsuranceSalary",
                table: "Employees");
        }
    }
}
