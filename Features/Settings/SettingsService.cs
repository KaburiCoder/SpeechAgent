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
    void UpdateSettings(string? connectKey = null, string? targetAppName = null, string? customExeTitle = null, string? customChartClass = null, string? customChartIndex = null, string? customNameClass = null, string? customNameIndex = null, string? customImageClass = null, string? customImageRect = null);
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

    public void UpdateSettings(string? connectKey = null, string? targetAppName = null, string? customExeTitle = null, string? customChartClass = null, string? customChartIndex = null, string? customNameClass = null, string? customNameIndex = null, string? customImageClass = null, string? customImageRect = null)
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
      if (targetAppName != null)
        currentSetting.TargetAppName = targetAppName.Trim();
      if (customExeTitle != null)
        currentSetting.CustomExeTitle = customExeTitle.Trim();
      if (customChartClass != null)
        currentSetting.CustomChartClass = customChartClass.Trim();
      if (customChartIndex != null)
        currentSetting.CustomChartIndex = customChartIndex.Trim();
      if (customNameClass != null)
        currentSetting.CustomNameClass = customNameClass.Trim();
      if (customNameIndex != null)
        currentSetting.CustomNameIndex = customNameIndex.Trim();
      if (customImageClass != null)
        currentSetting.CustomImageClass = customImageClass.Trim();
      if (customImageRect != null)
        currentSetting.CustomImageRect = customImageRect.Trim();

      if (dbSetting == null)
        db.LocalSettings.Add(currentSetting);

      db.SaveChanges();

      // Settings 속성 업데이트
      Settings = currentSetting.DeepCopy();

      WeakReferenceMessenger.Default.Send(new LocalSettingsChangedMessage(new LocalSettingsChangeData(currentSetting, previousSettings)));
    }
  }
}
