using System;
using System.IO;
using System.Text.Json;

namespace SpeechAgent.Features.Settings
{
  public class UserSettings
  {
    public string ConnectKey { get; set; } = "";
    public string AppName { get; set; } = "";
  }

  public static class UserSettingsManager
  {
    private static readonly string SettingsPath =
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SpeechAgent", "settings.json");

    public static UserSettings Load()
    {
      if (File.Exists(SettingsPath))
      {
        var json = File.ReadAllText(SettingsPath);
        return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
      }
      return new UserSettings();
    }

    public static void Save(UserSettings settings)
    {
      var dir = Path.GetDirectoryName(SettingsPath);
      if (!Directory.Exists(dir)) Directory.CreateDirectory(dir!);
      var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
      File.WriteAllText(SettingsPath, json);
    }
  }

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
    private UserSettings _settings;
    public string ConnectKey => _settings.ConnectKey;
    public string AppName => _settings.AppName;

    public SettingsService()
    {
      _settings = UserSettingsManager.Load();
    }

    public event Action<ISettingsService>? OnSettingChanged;
    public event Action<string> OnConnectKeyChanged = default!;

    public void UpdateSettings(string? connectKey = null, string? appName = null)
    {
      string previousConnectKey = _settings.ConnectKey;
      if (connectKey != null)
      {
        _settings.ConnectKey = connectKey.Trim();
      }
      if (appName != null)
      {
        _settings.AppName = appName;
      }
      UserSettingsManager.Save(_settings);
      if (previousConnectKey != _settings.ConnectKey)
        OnConnectKeyChanged?.Invoke(_settings.ConnectKey);
      OnSettingChanged?.Invoke(this);
    }
  }
}
