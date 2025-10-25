using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeechAgent.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomChartClass",
                table: "LocalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomChartIndex",
                table: "LocalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomExeTitle",
                table: "LocalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomNameClass",
                table: "LocalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomNameIndex",
                table: "LocalSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomChartClass",
                table: "LocalSettings");

            migrationBuilder.DropColumn(
                name: "CustomChartIndex",
                table: "LocalSettings");

            migrationBuilder.DropColumn(
                name: "CustomExeTitle",
                table: "LocalSettings");

            migrationBuilder.DropColumn(
                name: "CustomNameClass",
                table: "LocalSettings");

            migrationBuilder.DropColumn(
                name: "CustomNameIndex",
                table: "LocalSettings");
        }
    }
}
