using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContactRecordsToHaveSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloodReports_ContactRecords_ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.DropIndex(
                name: "IX_FloodReports_ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.DropColumn(
                name: "ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.DropColumn(
                name: "ContactName",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropColumn(
                name: "ContactType",
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

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.CreateTable(
                name: "ContactSubscribeRecords",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRecordOwner = table.Column<bool>(type: "boolean", nullable: false),
                    ContactType = table.Column<int>(type: "integer", nullable: false),
                    ContactName = table.Column<string>(type: "text", nullable: false),
                    EmailAddress = table.Column<string>(type: "text", nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    IsSubscribed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RedactionDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    VerificationCode = table.Column<int>(type: "integer", nullable: true),
                    VerificationExpiryUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                name: "IX_ContactRecords_ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "ContactUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactSubscribeRecords_ContactRecordId",
                schema: "fortpublic",
                table: "ContactSubscribeRecords",
                column: "ContactRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactSubscribeRecords",
                schema: "fortpublic");

            migrationBuilder.DropIndex(
                name: "IX_ContactRecords_ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ContactType",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "ReportOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloodReports_ContactRecords_ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "ReportOwnerId",
                principalSchema: "fortpublic",
                principalTable: "ContactRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
