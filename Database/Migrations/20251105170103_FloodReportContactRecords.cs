using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class FloodReportContactRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRecords_FloodReports_FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropTable(
                name: "FloodReportContactRecords",
                schema: "fortpublic");

            migrationBuilder.DropIndex(
                name: "IX_FloodReports_EligibilityCheckId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.DropIndex(
                name: "IX_ContactRecords_FloodReportId_UniqueWhenNoUser",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropColumn(
                name: "FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.CreateTable(
                name: "ContactRecordFloodReport",
                schema: "fortpublic",
                columns: table => new
                {
                    ContactRecordsId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodReportsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRecordFloodReport", x => new { x.ContactRecordsId, x.FloodReportsId });
                    table.ForeignKey(
                        name: "FK_ContactRecordFloodReport_ContactRecords_ContactRecordsId",
                        column: x => x.ContactRecordsId,
                        principalSchema: "fortpublic",
                        principalTable: "ContactRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactRecordFloodReport_FloodReports_FloodReportsId",
                        column: x => x.FloodReportsId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_EligibilityCheckId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "EligibilityCheckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactRecordFloodReport_FloodReportsId",
                schema: "fortpublic",
                table: "ContactRecordFloodReport",
                column: "FloodReportsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactRecordFloodReport",
                schema: "fortpublic");

            migrationBuilder.DropIndex(
                name: "IX_FloodReports_EligibilityCheckId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.AddColumn<Guid>(
                name: "FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FloodReportContactRecords",
                schema: "fortpublic",
                columns: table => new
                {
                    FloodReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactRecordId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodReportContactRecords", x => new { x.FloodReportId, x.ContactRecordId });
                    table.ForeignKey(
                        name: "FK_FloodReportContactRecord_ContactRecords_ContactRecordId",
                        column: x => x.ContactRecordId,
                        principalSchema: "fortpublic",
                        principalTable: "ContactRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloodReportContactRecord_FloodReports_FloodReportId",
                        column: x => x.FloodReportId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_EligibilityCheckId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "EligibilityCheckId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRecords_FloodReportId_UniqueWhenNoUser",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "FloodReportId",
                unique: true,
                filter: "\"ContactUserId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportContactRecords_ContactRecordId",
                schema: "fortpublic",
                table: "FloodReportContactRecords",
                column: "ContactRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRecords_FloodReports_FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "FloodReportId",
                principalSchema: "fortpublic",
                principalTable: "FloodReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
