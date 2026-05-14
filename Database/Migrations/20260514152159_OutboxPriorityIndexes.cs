using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class OutboxPriorityIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Status_Created",
                schema: "fortpublic",
                table: "OutboxMessages");

            migrationBuilder.AddColumn<string>(
                name: "ErrorReason",
                schema: "fortpublic",
                table: "OutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "fortpublic",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_Priority_Created",
                schema: "fortpublic",
                table: "OutboxMessages",
                columns: new[] { "Status", "Priority", "Created" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Status_Priority_Created",
                schema: "fortpublic",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "ErrorReason",
                schema: "fortpublic",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "fortpublic",
                table: "OutboxMessages");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_Created",
                schema: "fortpublic",
                table: "OutboxMessages",
                columns: new[] { "Status", "Created" });
        }
    }
}
