using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class ImportTablesAndSeedValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FailedImports",
                schema: "fortpublic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FailedImports", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodImpacts",
                columns: new[] { "Id", "Category", "CategoryPriority", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[] { new Guid("019f6a3a-f7eb-74ef-824d-5b673146160e"), "All", "Imported record with unknown value", 99, null, "Unknown" });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodMitigations",
                columns: new[] { "Id", "Category", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[] { new Guid("019f6a3a-f7eb-7c43-97d8-2c7cfa9d79e3"), "All", 99, null, "Imported record with unknown value" });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodProblems",
                columns: new[] { "Id", "Category", "OptionOrder", "TypeDescription", "TypeName" },
                values: new object[] { new Guid("019f6a3a-f7eb-748d-a169-a057be99f012"), "All", 99, "Imported record with unknown value", "Unknown" });

            migrationBuilder.InsertData(
                schema: "fortpublic",
                table: "FloodAuthorityFloodProblems",
                columns: new[] { "FloodAuthorityId", "FloodProblemId" },
                values: new object[] { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("019f6a3a-f7eb-748d-a169-a057be99f012") });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FailedImports",
                schema: "fortpublic");

            migrationBuilder.DeleteData(
                schema: "fortpublic",
                table: "FloodAuthorityFloodProblems",
                keyColumns: new[] { "FloodAuthorityId", "FloodProblemId" },
                keyValues: new object[] { new Guid("018fd119-7e60-7384-bb2b-c157b8b576c6"), new Guid("019f6a3a-f7eb-748d-a169-a057be99f012") });

            migrationBuilder.DeleteData(
                schema: "fortpublic",
                table: "FloodImpacts",
                keyColumn: "Id",
                keyValue: new Guid("019f6a3a-f7eb-74ef-824d-5b673146160e"));

            migrationBuilder.DeleteData(
                schema: "fortpublic",
                table: "FloodMitigations",
                keyColumn: "Id",
                keyValue: new Guid("019f6a3a-f7eb-7c43-97d8-2c7cfa9d79e3"));

            migrationBuilder.DeleteData(
                schema: "fortpublic",
                table: "FloodProblems",
                keyColumn: "Id",
                keyValue: new Guid("019f6a3a-f7eb-748d-a169-a057be99f012"));
        }
    }
}
