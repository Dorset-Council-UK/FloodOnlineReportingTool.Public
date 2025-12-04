using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenamedSubscribeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactSubscriptionRecords",
                schema: "fortpublic");

            migrationBuilder.CreateTable(
                name: "ContactSubscribeRecords",
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
                    table.PrimaryKey("PK_ContactSubscribeRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactSubscribeRecords_ContactRecords_ContactRecordId",
                        column: x => x.ContactRecordId,
                        principalSchema: "fortpublic",
                        principalTable: "ContactRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactSubscribeRecords_ContactRecordId",
                schema: "fortpublic",
                table: "ContactSubscribeRecords",
                column: "ContactRecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactSubscribeRecords",
                schema: "fortpublic");

            migrationBuilder.CreateTable(
                name: "ContactSubscriptionRecords",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContactRecordId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactName = table.Column<string>(type: "text", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsSubscribed = table.Column<bool>(type: "boolean", nullable: false),
                    RedactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerificationCode = table.Column<int>(type: "integer", nullable: true),
                    VerificationExpiryUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                name: "IX_ContactSubscriptionRecords_ContactRecordId",
                schema: "fortpublic",
                table: "ContactSubscriptionRecords",
                column: "ContactRecordId",
                unique: true);
        }
    }
}
