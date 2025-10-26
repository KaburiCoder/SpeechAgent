using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Constants;
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
    void UpdateSettings(string? connectKey = null, string? targetAppName = null, string? customExeTitle = null, string? customChartControlType = null, string? customChartIndex = null, string? customNameControlType = null, string? customNameIndex = null, string? customImageRect = null);

    bool UseCustomUserImage { get; }
  }

  public class SettingsService : ISettingsService
  {

    public LocalSettings Settings { get; private set; } = new();

    public bool UseCustomUserImage => Settings.TargetAppName == AppKey.CustomUserImage;

    public void LoadSettings()
    {
      using var db = new AppDbContext();

      LocalSettings? dbSetting = db.LocalSettings.FirstOrDefault();

      Settings = dbSetting?.DeepCopy() ?? new LocalSettings();
    }

    public void UpdateSettings(string? connectKey = null, string? targetAppName = null, string? customExeTitle = null, string? customChartControlType = null, string? customChartIndex = null, string? customNameControlType = null, string? customNameIndex = null, string? customImageRect = null)
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
      if (customChartControlType != null)
        currentSetting.CustomChartControlType = customChartControlType.Trim();
      if (customChartIndex != null)
        currentSetting.CustomChartIndex = customChartIndex.Trim();
      if (customNameControlType != null)
        currentSetting.CustomNameControlType = customNameControlType.Trim();
      if (customNameIndex != null)
        currentSetting.CustomNameIndex = customNameIndex.Trim();
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
