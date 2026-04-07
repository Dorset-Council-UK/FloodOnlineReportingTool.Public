using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class SetupContactModelConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContactRecords_ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRecords_ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "ContactUserId",
                unique: true,
                filter: "\"ContactUserId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ContactRecords_ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.CreateIndex(
                name: "IX_ContactRecords_ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "ContactUserId");
        }
    }
}
