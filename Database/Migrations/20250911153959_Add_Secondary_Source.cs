using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class Add_Secondary_Source : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateIndex(
                name: "IX_EligibilityCheckRunoffSource_FloodProblemId",
                schema: "fortpublic",
                table: "EligibilityCheckRunoffSource",
                column: "FloodProblemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EligibilityCheckRunoffSource",
                schema: "fortpublic");
        }
    }
}
