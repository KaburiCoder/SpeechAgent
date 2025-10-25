using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeechAgent.Migrations
{
    /// <inheritdoc />
    public partial class InitCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConnectKey = table.Column<string>(type: "TEXT", nullable: false),
                    TargetAppName = table.Column<string>(type: "TEXT", nullable: false),
                    CustomExeTitle = table.Column<string>(type: "TEXT", nullable: false),
                    CustomChartClass = table.Column<string>(type: "TEXT", nullable: false),
                    CustomChartIndex = table.Column<string>(type: "TEXT", nullable: false),
                    CustomNameClass = table.Column<string>(type: "TEXT", nullable: false),
                    CustomNameIndex = table.Column<string>(type: "TEXT", nullable: false),
                    CustomImageName = table.Column<string>(type: "TEXT", nullable: false),
                    CustomImageRect = table.Column<string>(type: "TEXT", nullable: false),
                    UseAutomation = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocalSettings");
        }
    }
}
