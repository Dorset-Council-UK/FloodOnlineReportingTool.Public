using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class ChangeContactRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRecords_FloodReports_FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.AlterColumn<Guid>(
                name: "FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid[]>(
                name: "FloodReportIds",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "Oid",
                schema: "fortpublic",
                table: "ContactRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRecords_FloodReports_FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "FloodReportId",
                principalSchema: "fortpublic",
                principalTable: "FloodReports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactRecords_FloodReports_FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropColumn(
                name: "FloodReportIds",
                schema: "fortpublic",
                table: "ContactRecords");

            migrationBuilder.DropColumn(
                name: "Oid",
                schema: "fortpublic",
                table: "ContactRecords");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ContactRecords_FloodReports_FloodReportId",
                schema: "fortpublic",
                table: "ContactRecords",
                column: "FloodReportId",
                principalSchema: "fortpublic",
                principalTable: "FloodReports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
