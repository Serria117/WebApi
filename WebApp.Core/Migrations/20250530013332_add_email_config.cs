using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class add_email_config : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_CostDepartment_CostDepartmentId",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CostDepartment",
                table: "CostDepartment");

            migrationBuilder.RenameTable(
                name: "CostDepartment",
                newName: "CostDepartments");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CostDepartments",
                table: "CostDepartments",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmailConfigs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AppPassword = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailConfigs", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_CostDepartments_CostDepartmentId",
                table: "Employees",
                column: "CostDepartmentId",
                principalTable: "CostDepartments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_CostDepartments_CostDepartmentId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "EmailConfigs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CostDepartments",
                table: "CostDepartments");

            migrationBuilder.RenameTable(
                name: "CostDepartments",
                newName: "CostDepartment");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CostDepartment",
                table: "CostDepartment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_CostDepartment_CostDepartmentId",
                table: "Employees",
                column: "CostDepartmentId",
                principalTable: "CostDepartment",
                principalColumn: "Id");
        }
    }
}
