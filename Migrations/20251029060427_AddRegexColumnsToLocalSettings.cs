using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeechAgent.Migrations
{
    /// <inheritdoc />
    public partial class AddRegexColumnsToLocalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomChartRegex",
                table: "LocalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CustomChartRegexIndex",
                table: "LocalSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomNameRegex",
                table: "LocalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CustomNameRegexIndex",
                table: "LocalSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomChartRegex",
                table: "LocalSettings");

            migrationBuilder.DropColumn(
                name: "CustomChartRegexIndex",
                table: "LocalSettings");

            migrationBuilder.DropColumn(
                name: "CustomNameRegex",
                table: "LocalSettings");

            migrationBuilder.DropColumn(
                name: "CustomNameRegexIndex",
                table: "LocalSettings");
        }
    }
}
