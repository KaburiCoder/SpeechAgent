using System.Configuration;

namespace SpeechAgent.Features.Settings
{

  public interface ISettingsService
  {
    event Action<ISettingsService>? OnSettingChanged;
    event Action<string> OnConnectKeyChanged;
    string ConnectKey { get; }
    string AppName { get; }

    void UpdateSettings(string connectKey = "", string appName = "");
  }

  public class SettingsService : ISettingsService
  {
    public string ConnectKey => setting.Default.CONNECT_KEY;
    public string AppName => setting.Default.APP_NAME;

    public SettingsService()
    {
    }

    public event Action<ISettingsService>? OnSettingChanged;
    public event Action<string> OnConnectKeyChanged = default!;

    public void UpdateSettings(string? connectKey = null, string? appName = null)
    {
      string previousConnectKey = setting.Default.CONNECT_KEY;

      if (connectKey != null)
      {
        setting.Default.CONNECT_KEY = connectKey.Trim();
      }

      if (appName != null)
      {
        setting.Default.APP_NAME = appName;
      }

      setting.Default.Save();

      if (previousConnectKey != setting.Default.CONNECT_KEY)
        OnConnectKeyChanged?.Invoke(setting.Default.CONNECT_KEY);
      OnSettingChanged?.Invoke(this);
    }
  }

  public static class SettingsMigrationHelper
  {
    public static void MigrateUserSettingsIfNeeded()
    {
      if (setting.Default.UpgradeRequired)
      {
        setting.Default.Upgrade();
        setting.Default.UpgradeRequired = false;
        setting.Default.Save();
      }
    }
  }
}
