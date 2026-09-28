using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogVerse.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizationColumnsToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                table: "DashboardPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomWidgetsJson",
                table: "DashboardPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DashboardStyle",
                table: "DashboardPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "DashboardPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThemeColor",
                table: "DashboardPreferences",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccentColor",
                table: "DashboardPreferences");

            migrationBuilder.DropColumn(
                name: "CustomWidgetsJson",
                table: "DashboardPreferences");

            migrationBuilder.DropColumn(
                name: "DashboardStyle",
                table: "DashboardPreferences");

            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "DashboardPreferences");

            migrationBuilder.DropColumn(
                name: "ThemeColor",
                table: "DashboardPreferences");
        }
    }
}
