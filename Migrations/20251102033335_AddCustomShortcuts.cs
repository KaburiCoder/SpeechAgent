using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeechAgent.Migrations
{
  /// <inheritdoc />
  public partial class AddCustomShortcuts : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "CustomShortcuts",
        columns: table => new
        {
          Modifiers = table.Column<int>(type: "INTEGER", nullable: false),
          Key = table.Column<int>(type: "INTEGER", nullable: false),
          ShortcutFeature = table.Column<int>(type: "INTEGER", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_CustomShortcuts", x => new { x.Modifiers, x.Key });
        }
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "CustomShortcuts");
    }
  }
}
