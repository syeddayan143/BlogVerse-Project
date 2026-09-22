using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogVerse.Migrations
{
    /// <inheritdoc />
    public partial class AddPostAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentName",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentName",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Posts");
        }
    }
}
