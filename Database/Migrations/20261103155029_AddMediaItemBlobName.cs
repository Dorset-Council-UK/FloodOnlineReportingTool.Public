using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodOnlineReportingTool.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMediaItemBlobName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "URL",
                schema: "fortpublic",
                table: "MediaItems",
                newName: "BlobName");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                schema: "fortpublic",
                table: "MediaItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            // Data fix: old rows stored a full SAS URL, e.g.
            // https://host/fort-staging/media/00fc2045-4e42-42f6-b867-844f454dcb60.jpg?sv=...
            // Keep from "media/" up to the "?" to get: media/00fc2045-4e42-42f6-b867-844f454dcb60.jpg
            migrationBuilder.Sql("""
                UPDATE fortpublic."MediaItems"
                SET "BlobName" = substring("BlobName" from '(media/[^?]+)')
                WHERE "BlobName" LIKE 'http%/media/%';
                """);

            // Derive ContentType from the extension, matching the allowed types in Index.razor.cs.
            migrationBuilder.Sql("""
                UPDATE fortpublic."MediaItems"
                SET "ContentType" = CASE lower(substring("BlobName" from '\.([^.]+)$'))
                    WHEN 'jpg'  THEN 'image/jpeg'
                    WHEN 'jpeg' THEN 'image/jpeg'
                    WHEN 'png'  THEN 'image/png'
                    WHEN 'gif'  THEN 'image/gif'
                    WHEN 'mp4'  THEN 'video/mp4'
                    WHEN 'webm' THEN 'video/webm'
                    WHEN 'ogg'  THEN 'video/ogg'
                    WHEN 'pdf'  THEN 'application/pdf'
                    ELSE ''
                END
                WHERE "ContentType" = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                schema: "fortpublic",
                table: "MediaItems");

            migrationBuilder.RenameColumn(
                name: "BlobName",
                schema: "fortpublic",
                table: "MediaItems",
                newName: "URL");
        }
    }
}
