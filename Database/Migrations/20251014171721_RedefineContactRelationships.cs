using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class RedefineContactRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserAccessUntilUtc",
                schema: "fortpublic",
                table: "FloodReports",
                newName: "ReportOwnerAccessUntil");

            migrationBuilder.RenameColumn(
                name: "ReportedByUserId",
                schema: "fortpublic",
                table: "FloodReports",
                newName: "ReportOwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_FloodReports_ReportedByUserId",
                schema: "fortpublic",
                table: "FloodReports",
                newName: "IX_FloodReports_ReportOwnerId");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactRecordId",
                schema: "fortpublic",
                table: "FloodReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloodReports_ContactRecordId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "ContactRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloodReports_ContactRecords_ContactRecordId",
                schema: "fortpublic",
                table: "FloodReports",
                column: "ContactRecordId",
                principalSchema: "fortpublic",
                principalTable: "ContactRecords",
                principalColumn: "Id");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloodReports_ContactRecords_ContactRecordId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FloodReports_ContactRecords_ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.DropIndex(
                name: "IX_FloodReports_ContactRecordId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.DropColumn(
                name: "ContactRecordId",
                schema: "fortpublic",
                table: "FloodReports");

            migrationBuilder.DropColumn(
                name: "ContactUserId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.RenameColumn(
                name: "ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports",
                newName: "ReportedByUserId");

            migrationBuilder.RenameColumn(
                name: "ReportOwnerAccessUntil",
                schema: "fortpublic",
                table: "FloodReports",
                newName: "UserAccessUntilUtc");

            migrationBuilder.RenameIndex(
                name: "IX_FloodReports_ReportOwnerId",
                schema: "fortpublic",
                table: "FloodReports",
                newName: "IX_FloodReports_ReportedByUserId");

            migrationBuilder.AlterColumn<Guid>(
                name: "FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
