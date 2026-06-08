using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class FloodReportSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "RecordStatuses",
                schema: "fortpublic",
                comment: "Status used in various places including flood report sources.",
                oldComment: "Status used in various places including flood reports.");

            migrationBuilder.AlterTable(
                name: "FloodResponsibilities",
                schema: "fortpublic",
                comment: "Areas responsible for handling flood report sources/eligibility checks.",
                oldComment: "Areas responsible for handling flood reports/eligibility checks.");

            migrationBuilder.CreateTable(
                name: "FloodReportSources",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MarkedForDeletionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("018feb10-38e0-7f30-a546-37ce71f243ae")),
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportOwnerAccessUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodReportSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloodReportSources_EligibilityChecks_EligibilityCheckId",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FloodReportSources_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FloodReportSources_RecordStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Flood report sources");

            migrationBuilder.CreateTable(
                name: "ContactRecordFloodReportSource",
                schema: "fortpublic",
                columns: table => new
                {
                    ContactRecordsId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodReportSourcesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactRecordFloodReportSource", x => new { x.ContactRecordsId, x.FloodReportSourcesId });
                    table.ForeignKey(
                        name: "FK_ContactRecordFloodReportSource_ContactRecords_ContactRecord~",
                        column: x => x.ContactRecordsId,
                        principalSchema: "fortpublic",
                        principalTable: "ContactRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactRecordFloodReportSource_FloodReportSources_FloodRepo~",
                        column: x => x.FloodReportSourcesId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodReportSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactRecordFloodReportSource_FloodReportSourcesId",
                schema: "fortpublic",
                table: "ContactRecordFloodReportSource",
                column: "FloodReportSourcesId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportSources_EligibilityCheckId",
                schema: "fortpublic",
                table: "FloodReportSources",
                column: "EligibilityCheckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportSources_Id",
                schema: "fortpublic",
                table: "FloodReportSources",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportSources_InvestigationId",
                schema: "fortpublic",
                table: "FloodReportSources",
                column: "InvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportSources_Reference",
                schema: "fortpublic",
                table: "FloodReportSources",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportSources_StatusId",
                schema: "fortpublic",
                table: "FloodReportSources",
                column: "StatusId");

            // copy data from old tables to new tables
            migrationBuilder.Sql("""
                INSERT INTO fortpublic."FloodReportSources"
                    ("Id", "Reference", "CreatedUtc", "MarkedForDeletionUtc", "StatusId", "EligibilityCheckId", "InvestigationId", "ReportOwnerAccessUntil")
                SELECT
                    "Id", "Reference", "CreatedUtc", "MarkedForDeletionUtc", "StatusId", "EligibilityCheckId", "InvestigationId", "ReportOwnerAccessUntil"
                FROM fortpublic."FloodReports";
            """);

            migrationBuilder.Sql("""
                INSERT INTO fortpublic."ContactRecordFloodReportSource"
                    ("ContactRecordsId", "FloodReportSourcesId")
                SELECT
                    "ContactRecordsId", "FloodReportsId"
                FROM fortpublic."ContactRecordFloodReport";
            """);

            // drop old tables
            migrationBuilder.DropTable(
                name: "ContactRecordFloodReport",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodReports",
                schema: "fortpublic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "RecordStatuses",
                schema: "fortpublic",
                comment: "Status used in various places including flood reports.",
                oldComment: "Status used in various places including flood report sources.");

            migrationBuilder.AlterTable(
                name: "FloodResponsibilities",
                schema: "fortpublic",
                comment: "Areas responsible for handling flood reports/eligibility checks.",
                oldComment: "Areas responsible for handling flood report sources/eligibility checks.");

            migrationBuilder.CreateTable(
                name: "FloodReports",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: true),
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusId = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("018feb10-38e0-7f30-a546-37ce71f243ae")),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MarkedForDeletionUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reference = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    ReportOwnerAccessUntil = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloodReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloodReports_EligibilityChecks_EligibilityCheckId",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FloodReports_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FloodReports_RecordStatuses_StatusId",
                        column: x => x.StatusId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Flood report overviews");

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
                name: "IX_ContactRecordFloodReport_FloodReportsId",
                schema: "fortpublic",
                table: "ContactRecordFloodReport",
                column: "FloodReportsId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_EligibilityCheckId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "EligibilityCheckId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_Id",
                schema: "fortpublic",
                table: "FloodReports",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_InvestigationId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "InvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_Reference",
                schema: "fortpublic",
                table: "FloodReports",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_StatusId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "StatusId");

            // copy data from the new tables back to the old tables
            migrationBuilder.Sql("""
                INSERT INTO fortpublic."FloodReports"
                    ("Id", "Reference", "CreatedUtc", "MarkedForDeletionUtc", "StatusId", "EligibilityCheckId", "InvestigationId", "ReportOwnerAccessUntil")
                SELECT
                    "Id", "Reference", "CreatedUtc", "MarkedForDeletionUtc", "StatusId", "EligibilityCheckId", "InvestigationId", "ReportOwnerAccessUntil"
                FROM fortpublic."FloodReportSources";
            """);

            migrationBuilder.Sql("""
                INSERT INTO fortpublic."ContactRecordFloodReport"
                    ("ContactRecordsId", "FloodReportsId")
                SELECT
                    "ContactRecordsId", "FloodReportSourcesId"
                FROM fortpublic."ContactRecordFloodReportSource";
            """);

            // drop the new tables
            migrationBuilder.DropTable(
                name: "ContactRecordFloodReportSource",
                    schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "FloodReportSources",
                schema: "fortpublic");
        }

    }
}
