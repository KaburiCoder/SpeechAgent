using Microsoft.EntityFrameworkCore;
using SpeechAgent.Database.Schemas;

namespace SpeechAgent.Database
{
  public class AppDbContext : DbContext
  {
    public DbSet<LocalSettings> LocalSettings { get; set; }

    public string DbPath { get; }

    public AppDbContext()
    {
      var folder = Environment.SpecialFolder.LocalApplicationData;
      var path = Environment.GetFolderPath(folder);

      // WPF 프로젝트의 Assembly 이름을 동적으로 가져옴
      var projectName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "SpeechAgent";
      DbPath = System.IO.Path.Join(path, projectName, "settings.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

  }
}
