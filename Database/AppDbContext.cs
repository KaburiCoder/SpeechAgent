using System.IO;
using Microsoft.EntityFrameworkCore;
using SpeechAgent.Database.Schemas;

namespace SpeechAgent.Database
{
  public class AppDbContext : DbContext
  {
    public DbSet<LocalSettings> LocalSettings { get; set; }
    public DbSet<CustomShortcuts> CustomShortcuts { get; set; }

    public string DbPath { get; }

    public AppDbContext()
    {
      var folder = Environment.SpecialFolder.LocalApplicationData;
      var path = Environment.GetFolderPath(folder);

      // WPF 프로젝트의 Assembly 이름을 동적으로 가져옴
      var projectName =
        System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "SpeechAgent";
      var dirPath = Path.Join(path, projectName);
      DbPath = Path.Join(dirPath, "settings.db");

      // 폴더가 없는 경우 생성
      var di = new DirectoryInfo(dirPath);
      if (!di.Exists)
        di.Create();
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
      options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<CustomShortcuts>().HasKey(cs => new { cs.Modifiers, cs.Key });
    }
  }
}
