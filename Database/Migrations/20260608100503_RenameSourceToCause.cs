using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenameSourceToCause : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EligibilityCheckCauses",
                schema: "fortpublic",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityCheckCauses", x => new { x.EligibilityCheckId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_EligibilityCheckCauses_EligibilityChecks_EligibilityCheckId",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityCheckCauses_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between eligibility checks and cause flood problems");

            migrationBuilder.CreateTable(
                name: "EligibilityCheckRunoffCause",
                schema: "fortpublic",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityCheckRunoffCause", x => new { x.EligibilityCheckId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_EligibilityCheckRunoffCause_EligibilityChecks_EligibilityCh~",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityCheckRunoffCause_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between eligibility checks and cause runoff flood problems");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckCauses_FloodProblemId",
                schema: "fortpublic",
                table: "EligibilityCheckCauses",
                column: "FloodProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckRunoffCause_FloodProblemId",
                schema: "fortpublic",
                table: "EligibilityCheckRunoffCause",
                column: "FloodProblemId");

            // copy data from old tables to new tables
            migrationBuilder.Sql("""
                INSERT INTO fortpublic."EligibilityCheckRunoffCause"
                    ("EligibilityCheckId", "FloodProblemId")
                SELECT
                    "EligibilityCheckId", "FloodProblemId"
                FROM fortpublic."EligibilityCheckRunoffSource";
            """);

            migrationBuilder.Sql("""
                INSERT INTO fortpublic."EligibilityCheckCauses"
                    ("EligibilityCheckId", "FloodProblemId")
                SELECT
                    "EligibilityCheckId", "FloodProblemId"
                FROM fortpublic."EligibilityCheckSources";
            """);

            // drop old tables
            migrationBuilder.DropTable(
                name: "EligibilityCheckRunoffSource",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "EligibilityCheckSources",
                schema: "fortpublic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EligibilityCheckRunoffSource",
                schema: "fortpublic",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityCheckRunoffSource", x => new { x.EligibilityCheckId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_EligibilityCheckRunoffSource_EligibilityChecks_EligibilityC~",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityCheckRunoffSource_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between eligibility checks and source runoff flood problems");

            migrationBuilder.CreateTable(
                name: "EligibilityCheckSources",
                schema: "fortpublic",
                columns: table => new
                {
                    EligibilityCheckId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodProblemId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EligibilityCheckSources", x => new { x.EligibilityCheckId, x.FloodProblemId });
                    table.ForeignKey(
                        name: "FK_EligibilityCheckSources_EligibilityChecks_EligibilityCheckId",
                        column: x => x.EligibilityCheckId,
                        principalSchema: "fortpublic",
                        principalTable: "EligibilityChecks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EligibilityCheckSources_FloodProblems_FloodProblemId",
                        column: x => x.FloodProblemId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodProblems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between eligibility checks and source flood problems");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckRunoffSource_FloodProblemId",
                schema: "fortpublic",
                table: "EligibilityCheckRunoffSource",
                column: "FloodProblemId");

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckSources_FloodProblemId",
                schema: "fortpublic",
                table: "EligibilityCheckSources",
                column: "FloodProblemId");

            // copy data from the new tables back to the old tables
            migrationBuilder.Sql("""
                INSERT INTO fortpublic."EligibilityCheckRunoffSource"
                    ("EligibilityCheckId", "FloodProblemId")
                SELECT
                    "EligibilityCheckId", "FloodProblemId"
                FROM fortpublic."EligibilityCheckRunoffCause";
            """);

            migrationBuilder.Sql("""
                INSERT INTO fortpublic."EligibilityCheckSources"
                    ("EligibilityCheckId", "FloodProblemId")
                SELECT
                    "EligibilityCheckId", "FloodProblemId"
                FROM fortpublic."EligibilityCheckCauses";
            """);

            // drop the new tables
            migrationBuilder.DropTable(
                name: "EligibilityCheckCauses",
                schema: "fortpublic");

            migrationBuilder.DropTable(
                name: "EligibilityCheckRunoffCause",
                schema: "fortpublic");
        }
    }
}
