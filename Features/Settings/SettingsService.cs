using SpeechAgent.Database;
using SpeechAgent.Database.Schemas;

namespace SpeechAgent.Features.Settings
{

  public interface ISettingsService
  {
    event Action<ISettingsService>? OnSettingChanged;
  event Action<string> OnConnectKeyChanged;
    string ConnectKey { get; }
    string AppName { get; }
    void LoadSettings();
    void UpdateSettings(string connectKey = "", string appName = "");
  }

  public class SettingsService : ISettingsService
  {
    public string ConnectKey { get; private set; } = "";
    public string AppName { get; private set; } = "";

    public SettingsService()
    {
    }

    public event Action<ISettingsService>? OnSettingChanged;
    public event Action<string> OnConnectKeyChanged = default!;

    public void LoadSettings()
    {
      using var db = new AppDbContext();

      LocalSettings? dbSetting = db.LocalSettings.FirstOrDefault();
      ConnectKey = dbSetting?.ConnectKey ?? string.Empty;
      AppName = dbSetting?.TargetAppName ?? string.Empty;
    }

    public void UpdateSettings(string? connectKey = null, string? appName = null)
    {
      using var db = new AppDbContext();

      // EF Core에서 ConnectKey 불러오기
      LocalSettings? dbSetting = db.LocalSettings.FirstOrDefault();
      LocalSettings? currentSetting = null;
      string? previousConnectKey = dbSetting?.ConnectKey;

  currentSetting = dbSetting == null ? new LocalSettings() : dbSetting;

      if (connectKey != null)
        currentSetting.ConnectKey = connectKey.Trim();
   if (appName != null)
        currentSetting.TargetAppName = appName.Trim();

      if (dbSetting == null)
 db.LocalSettings.Add(currentSetting);

      db.SaveChanges();

      if (dbSetting?.ConnectKey != currentSetting.ConnectKey)
        OnConnectKeyChanged?.Invoke(currentSetting.ConnectKey);

      OnSettingChanged?.Invoke(this);
    }
  }
}
