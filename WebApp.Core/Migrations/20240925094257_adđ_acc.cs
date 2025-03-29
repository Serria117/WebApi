using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class adđ_acc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateSequence<int>(
                name: "CommonSeq",
                schema: "dbo");

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Parent = table.Column<int>(type: "int", nullable: true),
                    Grade = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BalanceSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    From = table.Column<DateTime>(type: "datetime2", nullable: false),
                    To = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceSheets_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BalanceSheetDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false, defaultValueSql: "NEXT VALUE FOR dbo.CommonSeq"),
                    Account = table.Column<int>(type: "int", nullable: false),
                    RecordTime = table.Column<int>(type: "int", nullable: false),
                    CreditValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebitValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BlId = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceSheetDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceSheetDetails_BalanceSheets_BlId",
                        column: x => x.BlId,
                        principalTable: "BalanceSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportedBalanceSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BalanceSheetId = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedBalanceSheets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportedBalanceSheets_BalanceSheets_BalanceSheetId",
                        column: x => x.BalanceSheetId,
                        principalTable: "BalanceSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportedBalanceSheetDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Account = table.Column<int>(type: "int", nullable: false),
                    RecordTime = table.Column<int>(type: "int", nullable: false),
                    CreditValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebitValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BlId = table.Column<int>(type: "int", nullable: false),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedBalanceSheetDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImportedBalanceSheetDetails_ImportedBalanceSheets_BlId",
                        column: x => x.BlId,
                        principalTable: "ImportedBalanceSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalanceSheetDetails_BlId",
                table: "BalanceSheetDetails",
                column: "BlId");

            migrationBuilder.CreateIndex(
                name: "IX_BalanceSheets_OrganizationId",
                table: "BalanceSheets",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedBalanceSheetDetails_BlId",
                table: "ImportedBalanceSheetDetails",
                column: "BlId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedBalanceSheets_BalanceSheetId",
                table: "ImportedBalanceSheets",
                column: "BalanceSheetId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "BalanceSheetDetails");

            migrationBuilder.DropTable(
                name: "ImportedBalanceSheetDetails");

            migrationBuilder.DropTable(
                name: "ImportedBalanceSheets");

            migrationBuilder.DropTable(
                name: "BalanceSheets");

            migrationBuilder.DropSequence(
                name: "CommonSeq",
                schema: "dbo");
        }
    }
}
