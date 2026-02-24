using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedPropertyInsuredToInvestigations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PropertyInsuredId",
                schema: "fortpublic",
                table: "Investigations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_PropertyInsuredId",
                schema: "fortpublic",
                table: "Investigations",
                column: "PropertyInsuredId");

            migrationBuilder.AddForeignKey(
                name: "FK_Investigations_RecordStatuses_PropertyInsuredId",
                schema: "fortpublic",
                table: "Investigations",
                column: "PropertyInsuredId",
                principalSchema: "fortpublic",
                principalTable: "RecordStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investigations_RecordStatuses_PropertyInsuredId",
                schema: "fortpublic",
                table: "Investigations");

            migrationBuilder.DropIndex(
                name: "IX_Investigations_PropertyInsuredId",
                schema: "fortpublic",
                table: "Investigations");

            migrationBuilder.DropColumn(
                name: "PropertyInsuredId",
                schema: "fortpublic",
                table: "Investigations");
        }
    }
}
