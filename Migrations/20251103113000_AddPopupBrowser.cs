using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpeechAgent.Migrations
{
  /// <inheritdoc />
  public partial class AddPopupBrowser : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<DateTime>(
        name: "BootPopupBrowserDate",
        table: "LocalSettings",
        type: "TEXT",
        nullable: false,
        defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
      );

      migrationBuilder.AddColumn<bool>(
        name: "IsBootPopupBrowserEnabled",
        table: "LocalSettings",
        type: "INTEGER",
        nullable: false,
        defaultValue: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "BootPopupBrowserDate", table: "LocalSettings");

      migrationBuilder.DropColumn(name: "IsBootPopupBrowserEnabled", table: "LocalSettings");
    }
  }
}
