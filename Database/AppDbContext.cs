using System.IO;
using Microsoft.EntityFrameworkCore;
using SpeechAgent.Database.Schemas;
using SpeechAgent.Utils;

namespace SpeechAgent.Database
{
  public class AppDbContext : DbContext
  {
    public DbSet<LocalSettings> LocalSettings { get; set; }
    public DbSet<CustomShortcuts> CustomShortcuts { get; set; }

    public string DbPath { get; }

    /// <summary>
    /// 컨텍스트 인스턴스화 없이 DB 파일 경로만 조회합니다. 디렉토리는 보장하지 않습니다.
    /// </summary>
    public static string GetDbPath()
    {
      return Path.Combine(PathUtils.GetLocalAppDataDirectory(), "settings.db");
    }

    public AppDbContext()
    {
      DbPath = GetDbPath();

      var dirPath = Path.GetDirectoryName(DbPath)!;
      var di = new DirectoryInfo(dirPath);
      if (!di.Exists)
        di.Create();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
      options.UseSqlite($"Data Source={DbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.Entity<CustomShortcuts>().HasKey(cs => cs.ShortcutFeature);

      // LocalSettings의 기본값 설정
      modelBuilder.Entity<LocalSettings>(entity =>
      {
        entity.Property(ls => ls.IsBootPopupBrowserEnabled).HasDefaultValue(true);
        entity.Property(ls => ls.AudioFileSaveDir).HasDefaultValue("C:\\VoiceMedic");
      });
    }
  }
}
