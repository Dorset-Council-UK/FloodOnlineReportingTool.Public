using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class PeakDepthDescriptionLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "InvestigationPeakDepths",
                schema: "fortpublic",
                comment: "Investigations peak depths");

            migrationBuilder.AlterColumn<string>(
                name: "DepthLocation",
                schema: "fortpublic",
                table: "InvestigationPeakDepths",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterTable(
                name: "InvestigationPeakDepths",
                schema: "fortpublic",
                oldComment: "Investigations peak depths");

            migrationBuilder.AlterColumn<string>(
                name: "DepthLocation",
                schema: "fortpublic",
                table: "InvestigationPeakDepths",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);
        }
    }
}
