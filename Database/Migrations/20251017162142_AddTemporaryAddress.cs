using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddTemporaryAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TemporaryLocationDesc",
                schema: "fortpublic",
                table: "EligibilityChecks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TemporaryUprn",
                schema: "fortpublic",
                table: "EligibilityChecks",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemporaryLocationDesc",
                schema: "fortpublic",
                table: "EligibilityChecks");

            migrationBuilder.DropColumn(
                name: "TemporaryUprn",
                schema: "fortpublic",
                table: "EligibilityChecks");
        }
    }
}
