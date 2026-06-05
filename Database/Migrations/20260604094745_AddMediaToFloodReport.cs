using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaToFloodReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MediaItems",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UploadDateUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    URL = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    FloodReportId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaItems_FloodReports_FloodReportId",
                        column: x => x.FloodReportId,
                        principalSchema: "fortpublic",
                        principalTable: "FloodReports",
                        principalColumn: "Id");
                },
                comment: "Media items linked to flood reports");

            migrationBuilder.CreateIndex(
                name: "IX_MediaItems_FloodReportId",
                schema: "fortpublic",
                table: "MediaItems",
                column: "FloodReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaItems",
                schema: "fortpublic");
        }
    }
}
