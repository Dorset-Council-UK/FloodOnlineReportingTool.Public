using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class ImpactOnSerices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestigationServiceImpact",
                schema: "fortpublic",
                columns: table => new
                {
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    FloodImpactId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationServiceImpact", x => new { x.InvestigationId, x.FloodImpactId });
                    table.ForeignKey(
                        name: "FK_InvestigationServiceImpact_FloodImpacts_FloodImpactId",
                        column: x => x.FloodImpactId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodImpacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationServiceImpact_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Relationships between investigation and service flood impacts");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71a-aa00-7ac0-b521-ccf27f194875"),
                column: "OptionOrder",
                value: 98);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71b-9460-715b-aa13-d9eabd5b7ef1"),
                column: "TypeName",
                value: "Private sewer");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71c-7ec0-7a1b-94a6-c7d7ae52b977"),
                column: "TypeName",
                value: "Mains sewer");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71d-6920-787b-ab3f-b6f251f4834b"),
                column: "TypeName",
                value: "Water supply");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd721-12a0-7341-a0fb-818543c14e0f"),
                column: "TypeName",
                value: "Not sure");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationServiceImpact_FloodImpactId",
                schema: "fortpublic",
                table: "InvestigationServiceImpact",
                column: "FloodImpactId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestigationServiceImpact",
                schema: "fortpublic");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71a-aa00-7ac0-b521-ccf27f194875"),
                column: "OptionOrder",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71b-9460-715b-aa13-d9eabd5b7ef1"),
                column: "TypeName",
                value: "Private Sewer");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71c-7ec0-7a1b-94a6-c7d7ae52b977"),
                column: "TypeName",
                value: "Mains Sewer");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd71d-6920-787b-ab3f-b6f251f4834b"),
                column: "TypeName",
                value: "Water Supply");

            migrationBuilder.UpdateData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("018fd721-12a0-7341-a0fb-818543c14e0f"),
                column: "TypeName",
                value: "Not Sure");
        }
    }
}
