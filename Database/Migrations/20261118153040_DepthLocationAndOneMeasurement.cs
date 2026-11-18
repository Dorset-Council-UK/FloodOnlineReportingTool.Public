using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class DepthLocationAndOneMeasurement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PeakInsideCentimetres",
                schema: "fortpublic",
                table: "InvestigationPeakDepths");

            migrationBuilder.RenameColumn(
                name: "PeakOutsideCentimetres",
                schema: "fortpublic",
                table: "InvestigationPeakDepths",
                newName: "PeakCentimetres");

            migrationBuilder.AddColumn<string>(
                name: "DepthLocation",
                schema: "fortpublic",
                table: "InvestigationPeakDepths",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepthLocation",
                schema: "fortpublic",
                table: "InvestigationPeakDepths");

            migrationBuilder.RenameColumn(
                name: "PeakCentimetres",
                schema: "fortpublic",
                table: "InvestigationPeakDepths",
                newName: "PeakOutsideCentimetres");

            migrationBuilder.AddColumn<int>(
                name: "PeakInsideCentimetres",
                schema: "fortpublic",
                table: "InvestigationPeakDepths",
                type: "integer",
                nullable: true);
        }
    }
}
