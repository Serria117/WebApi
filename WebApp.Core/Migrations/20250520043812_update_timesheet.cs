using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class update_timesheet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeSheets_PayrollRecordId",
                table: "TimeSheets");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSheets_PayrollRecordId_Date",
                table: "TimeSheets",
                columns: new[] { "PayrollRecordId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TimeSheets_PayrollRecordId_Date",
                table: "TimeSheets");

            migrationBuilder.CreateIndex(
                name: "IX_TimeSheets_PayrollRecordId",
                table: "TimeSheets",
                column: "PayrollRecordId");
        }
    }
}
