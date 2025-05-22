using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class payroll_tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayrollComponentCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollComponentCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollInputTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollInputTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayrollPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollPeriods_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollComponentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayrollComponentCategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollComponentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollComponentTypes_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PayrollComponentTypes_PayrollComponentCategories_PayrollComponentCategoryId",
                        column: x => x.PayrollComponentCategoryId,
                        principalTable: "PayrollComponentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalGrossPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDeduction = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetPay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PayrollPeriodId = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollRecords_PayrollPeriods_PayrollPeriodId",
                        column: x => x.PayrollPeriodId,
                        principalTable: "PayrollPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollInputs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayrollRecordId = table.Column<long>(type: "bigint", nullable: false),
                    PayrollInputTypeId = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollInputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollInputs_PayrollInputTypes_PayrollInputTypeId",
                        column: x => x.PayrollInputTypeId,
                        principalTable: "PayrollInputTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollInputs_PayrollRecords_PayrollRecordId",
                        column: x => x.PayrollRecordId,
                        principalTable: "PayrollRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayrollItems",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PayrollRecordId = table.Column<long>(type: "bigint", nullable: false),
                    PayrollComponentTypeId = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayrollItems_PayrollComponentTypes_PayrollComponentTypeId",
                        column: x => x.PayrollComponentTypeId,
                        principalTable: "PayrollComponentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PayrollItems_PayrollRecords_PayrollRecordId",
                        column: x => x.PayrollRecordId,
                        principalTable: "PayrollRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeSheets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PayrollRecordId = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsWeekend = table.Column<bool>(type: "bit", nullable: false),
                    IsHoliday = table.Column<bool>(type: "bit", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Leave = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeSheets_PayrollRecords_PayrollRecordId",
                        column: x => x.PayrollRecordId,
                        principalTable: "PayrollRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayrollComponentTypes_OrganizationId",
                table: "PayrollComponentTypes",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollComponentTypes_PayrollComponentCategoryId",
                table: "PayrollComponentTypes",
                column: "PayrollComponentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollInputs_PayrollInputTypeId",
                table: "PayrollInputs",
                column: "PayrollInputTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollInputs_PayrollRecordId",
                table: "PayrollInputs",
                column: "PayrollRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_PayrollComponentTypeId",
                table: "PayrollItems",
                column: "PayrollComponentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollItems_PayrollRecordId",
                table: "PayrollItems",
                column: "PayrollRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_Name",
                table: "PayrollPeriods",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollPeriods_OrganizationId",
                table: "PayrollPeriods",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_EmployeeId_PayrollPeriodId",
                table: "PayrollRecords",
                columns: new[] { "EmployeeId", "PayrollPeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayrollRecords_PayrollPeriodId",
                table: "PayrollRecords",
                column: "PayrollPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSheets_PayrollRecordId",
                table: "TimeSheets",
                column: "PayrollRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayrollInputs");

            migrationBuilder.DropTable(
                name: "PayrollItems");

            migrationBuilder.DropTable(
                name: "TimeSheets");

            migrationBuilder.DropTable(
                name: "PayrollInputTypes");

            migrationBuilder.DropTable(
                name: "PayrollComponentTypes");

            migrationBuilder.DropTable(
                name: "PayrollRecords");

            migrationBuilder.DropTable(
                name: "PayrollComponentCategories");

            migrationBuilder.DropTable(
                name: "PayrollPeriods");
        }
    }
}
