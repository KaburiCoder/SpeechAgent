using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Database;
using SpeechAgent.Database.Schemas;
using SpeechAgent.Messages;
using SpeechAgent.Utils;

namespace SpeechAgent.Features.Settings
{

  public interface ISettingsService
  {
    LocalSettings Settings { get; }
    void LoadSettings();
    void UpdateSettings(string connectKey = "", string targetAppName = "");
  }

  public class SettingsService : ISettingsService
  {

    public LocalSettings Settings { get; private set; } = new();

    public void LoadSettings()
    {
      using var db = new AppDbContext();

      LocalSettings? dbSetting = db.LocalSettings.FirstOrDefault();

      Settings = dbSetting?.DeepCopy() ?? new LocalSettings();      
    }

    public void UpdateSettings(string? connectKey = null, string? appName = null)
    {
      using var db = new AppDbContext();

      // EF Core에서 ConnectKey 불러오기
      LocalSettings? dbSetting = db.LocalSettings.FirstOrDefault();
      LocalSettings? currentSetting = null;
      string? previousConnectKey = dbSetting?.ConnectKey;

      currentSetting = dbSetting == null ? new LocalSettings() : dbSetting;
      LocalSettings previousSettings = currentSetting.DeepCopy();

      if (connectKey != null)
        currentSetting.ConnectKey = connectKey.Trim();
      if (appName != null)
        currentSetting.TargetAppName = appName.Trim();

      if (dbSetting == null)
        db.LocalSettings.Add(currentSetting);

      db.SaveChanges();

      // Settings 속성 업데이트
      Settings = currentSetting.DeepCopy();

      WeakReferenceMessenger.Default.Send(new LocalSettingsChangedMessage(new LocalSettingsChangeData(currentSetting, previousSettings)));
    }
  }
}
