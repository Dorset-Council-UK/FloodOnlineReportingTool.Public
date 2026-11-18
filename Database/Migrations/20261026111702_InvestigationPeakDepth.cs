using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class InvestigationPeakDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investigations_RecordStatuses_IsPeakDepthKnownId",
                schema: "fortpublic",
                table: "Investigations");

            migrationBuilder.DropIndex(
                name: "IX_Investigations_IsPeakDepthKnownId",
                schema: "fortpublic",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "IsPeakDepthKnownId",
                schema: "fortpublic",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "PeakInsideCentimetres",
                schema: "fortpublic",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "PeakOutsideCentimetres",
                schema: "fortpublic",
                table: "Investigations");

            migrationBuilder.AddColumn<bool>(
                name: "HasPeakDepth",
                schema: "fortpublic",
                table: "Investigations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "InvestigationPeakDepths",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvestigationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsPeakDepthKnownId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeakInsideCentimetres = table.Column<int>(type: "integer", nullable: true),
                    PeakOutsideCentimetres = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationPeakDepths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestigationPeakDepths_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalSchema: "fortpublic",
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationPeakDepths_RecordStatuses_IsPeakDepthKnownId",
                        column: x => x.IsPeakDepthKnownId,
                        principalSchema: "fortpublic",
                        principalTable: "RecordStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationPeakDepths_InvestigationId",
                schema: "fortpublic",
                table: "InvestigationPeakDepths",
                column: "InvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationPeakDepths_IsPeakDepthKnownId",
                schema: "fortpublic",
                table: "InvestigationPeakDepths",
                column: "IsPeakDepthKnownId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestigationPeakDepths",
                schema: "fortpublic");

            migrationBuilder.DropColumn(
                name: "HasPeakDepth",
                schema: "fortpublic",
                table: "Investigations");

            migrationBuilder.AddColumn<Guid>(
                name: "IsPeakDepthKnownId",
                schema: "fortpublic",
                table: "Investigations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "PeakInsideCentimetres",
                schema: "fortpublic",
                table: "Investigations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeakOutsideCentimetres",
                schema: "fortpublic",
                table: "Investigations",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_IsPeakDepthKnownId",
                schema: "fortpublic",
                table: "Investigations",
                column: "IsPeakDepthKnownId");

            migrationBuilder.AddForeignKey(
                name: "FK_Investigations_RecordStatuses_IsPeakDepthKnownId",
                schema: "fortpublic",
                table: "Investigations",
                column: "IsPeakDepthKnownId",
                principalSchema: "fortpublic",
                principalTable: "RecordStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
