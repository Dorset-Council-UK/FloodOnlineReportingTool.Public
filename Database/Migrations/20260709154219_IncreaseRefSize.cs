using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseRefSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FloodReportSources_Id",
                schema: "fortpublic",
                table: "FloodReportSources");

            migrationBuilder.DropIndex(
                name: "IX_FloodReportSources_Reference",
                schema: "fortpublic",
                table: "FloodReportSources");

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                schema: "fortpublic",
                table: "FloodReportSources",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_FloodReportSources_Reference",
                schema: "fortpublic",
                table: "FloodReportSources",
                column: "Reference");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_FloodReportSources_Reference",
                schema: "fortpublic",
                table: "FloodReportSources");

            migrationBuilder.AlterColumn<string>(
                name: "Reference",
                schema: "fortpublic",
                table: "FloodReportSources",
                type: "character varying(8)",
                maxLength: 8,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(15)",
                oldMaxLength: 15);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportSources_Id",
                schema: "fortpublic",
                table: "FloodReportSources",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReportSources_Reference",
                schema: "fortpublic",
                table: "FloodReportSources",
                column: "Reference",
                unique: true);
        }
    }
}
