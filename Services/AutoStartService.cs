using Microsoft.Win32;
using System.Reflection;

namespace SpeechAgent.Services
{
  public interface IAutoStartService
  {
    bool IsAutoStartEnabled();
    void SetAutoStart(bool enable);
  }

  public class AutoStartService : IAutoStartService
  {
    private const string AppName = "VoiceMedicAgent";
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public bool IsAutoStartEnabled()
    {
      try
      {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
        var value = key?.GetValue(AppName);
        return value != null;
      }
      catch
      {
        return false;
      }
    }

    public void SetAutoStart(bool enable)
    {
      try
      {
        IsAutoStartEnabled();
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
        if (key == null) return;

        if (enable)
        {
          var exePath = Environment.ProcessPath;
          key.SetValue(AppName, $"\"{exePath}\"");
        }
        else
        {
          key.DeleteValue(AppName, false);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Failed to set auto start: {ex.Message}");
      }
    }
  }
}
