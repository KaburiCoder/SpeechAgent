using System.IO;
using System.Reflection;
using Microsoft.Win32;
using SpeechAgent.Utils;
using WindowsShortcutFactory;

namespace SpeechAgent.Services
{
  public interface IAutoStartService
  {
    bool IsAutoStartEnabledLegacy();
    void SetAutoStartLegacy(bool enable);
    bool CreateStartupShortcut();
    bool IsAutoStartEnabled();
    bool DeleteStartupShortcut();
    void MigrateToShortcutIfNeeded();
  }

  public class AutoStartService : IAutoStartService
  {
    private const string AppName = "VoiceMedicAgent";
    private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public bool IsAutoStartEnabledLegacy()
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

    public void SetAutoStartLegacy(bool enable)
    {
      try
      {
        IsAutoStartEnabledLegacy();
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
        if (key == null)
          return;

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

    private static string GetSafeExecutablePath()
    {
      if (Environment.ProcessPath is { } processPath && File.Exists(processPath))
        return processPath;

      string baseDir = AppDomain.CurrentDomain.BaseDirectory;
      string exeName = AppDomain.CurrentDomain.FriendlyName;

      if (!exeName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        exeName += ".exe";

      string candidate = Path.Combine(baseDir, exeName);

      return candidate;
    }

    public bool CreateStartupShortcut()
    {
      try
      {
        string targetPath = GetSafeExecutablePath();
        string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string shortcutPath = Path.Combine(startupFolder, $"{AppName}.lnk");

        // 작업 디렉토리: 실행 파일이 있는 디렉토리
        string workingDirectory =
          Path.GetDirectoryName(targetPath) ?? AppDomain.CurrentDomain.BaseDirectory;

        using var shortcut = new WindowsShortcut
        {
          Path = targetPath,
          WorkingDirectory = workingDirectory,
          Description = $"{AppName} 자동 시작",
          IconLocation = targetPath,
        };

        shortcut.Save(shortcutPath);
        LogUtils.WriteLog(
          LogLevel.Info,
          $"[AutoStart] 시작 폴더 바로가기 생성 완료: {shortcutPath}"
        );
        return true;
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(
          LogLevel.Error,
          $"[AutoStart] 시작 폴더 바로가기 생성 실패: {ex.Message}"
        );
        return false;
      }
    }

    /// <summary>
    /// 시작 폴더의 바로가기를 확인합니다.
    /// </summary>
    public bool IsAutoStartEnabled()
    {
      try
      {
        string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string targetPath = GetSafeExecutablePath();
        string shortcutPath = Path.Combine(startupFolder, $"{AppName}.lnk");

        bool exists = File.Exists(shortcutPath);
        LogUtils.WriteLog(
          LogLevel.Debug,
          $"[AutoStart] 시작 폴더 바로가기 확인: {(exists ? "존재" : "미존재")} ({shortcutPath})"
        );
        return exists;
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(
          LogLevel.Error,
          $"[AutoStart] 시작 폴더 바로가기 확인 실패: {ex.Message}"
        );
        return false;
      }
    }

    /// <summary>
    /// 시작 폴더의 바로가기를 삭제합니다.
    /// </summary>
    public bool DeleteStartupShortcut()
    {
      try
      {
        string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string targetPath = GetSafeExecutablePath();
        string appName = Path.GetFileNameWithoutExtension(targetPath);
        string shortcutPath = Path.Combine(startupFolder, $"{appName}.lnk");

        if (File.Exists(shortcutPath))
        {
          File.Delete(shortcutPath);
          LogUtils.WriteLog(
            LogLevel.Info,
            $"[AutoStart] 시작 폴더 바로가기 삭제 완료: {shortcutPath}"
          );
          return true;
        }

        LogUtils.WriteLog(
          LogLevel.Debug,
          $"[AutoStart] 삭제할 바로가기가 없습니다: {shortcutPath}"
        );
        return true;
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(
          LogLevel.Error,
          $"[AutoStart] 시작 폴더 바로가기 삭제 실패: {ex.Message}"
        );
        return false;
      }
    }

    /// <summary>
    /// 레지스트리의 레거시 자동 시작 설정을 바로가기 방식으로 마이그레이션합니다.
    /// 레지스트리에 등록되어 있으면 레지스트리에서 제거하고 바로가기를 생성합니다.
    /// </summary>
    public void MigrateToShortcutIfNeeded()
    {
      try
      {
        LogUtils.WriteLog(
          LogLevel.Debug,
          "[AutoStart] 레거시 자동 시작 설정 마이그레이션 확인 중..."
        );

        // 레지스트리에 자동 시작이 설정되어 있는지 확인
        if (IsAutoStartEnabledLegacy())
        {
          LogUtils.WriteLog(
            LogLevel.Info,
            "[AutoStart] 레거시 자동 시작 설정 감지됨. 바로가기 방식으로 마이그레이션 시작..."
          );

          // 1. 레지스트리에서 제거
          SetAutoStartLegacy(false);
          LogUtils.WriteLog(LogLevel.Info, "[AutoStart] 레지스트리에서 자동 시작 설정 제거 완료");

          // 2. 바로가기 생성
          if (CreateStartupShortcut())
          {
            LogUtils.WriteLog(
              LogLevel.Info,
              "[AutoStart] 레거시 설정 마이그레이션 완료. 바로가기 방식으로 전환됨"
            );
          }
          else
          {
            LogUtils.WriteLog(
              LogLevel.Error,
              "[AutoStart] 바로가기 생성에 실패했습니다. 수동으로 다시 시도해주세요."
            );
          }
        }
        else
        {
          LogUtils.WriteLog(
            LogLevel.Debug,
            "[AutoStart] 마이그레이션 필요 없음. 레거시 설정이 없습니다."
          );
        }
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, $"[AutoStart] 마이그레이션 중 오류 발생: {ex.Message}");
      }
    }
  }
}
