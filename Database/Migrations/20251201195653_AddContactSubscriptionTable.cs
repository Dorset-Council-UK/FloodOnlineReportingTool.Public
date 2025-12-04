using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddContactSubscriptionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactName",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropColumn(
                name: "EmailAddress",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactSubscriptionRecord",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ContactSubscriptionRecords",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsSubscribed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RedactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerificationCode = table.Column<int>(type: "integer", nullable: true),
                    VerificationExpiryUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ContactRecordId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactSubscriptionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactSubscriptionRecords_ContactRecords_ContactRecordId",
                        column: x => x.ContactRecordId,
                        principalSchema: "fortpublic",
                        principalTable: "ContactRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactRecords_ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "ContactUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactSubscriptionRecords_ContactRecordId",
                schema: "fortpublic",
                table: "ContactSubscriptionRecords",
                column: "ContactRecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactSubscriptionRecords",
                schema: "fortpublic");

            migrationBuilder.DropIndex(
                name: "IX_ContactRecords_ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropColumn(
                name: "ContactSubscriptionRecord",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
