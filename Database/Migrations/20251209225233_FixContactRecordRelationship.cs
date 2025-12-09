using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class FixContactRecordRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ContactSubscriptionRecord",
                schema: "fortpublic",
                table: "ContactRecords",
                newName: "SubscribeRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubscribeRecordId",
                schema: "fortpublic",
                table: "ContactRecords",
                newName: "ContactSubscriptionRecord");
        }
    }
}
