using System.Diagnostics;
using System.Security.Principal;
using System.Windows;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// 관리자 권한 관련 유틸리티 클래스입니다.
  /// </summary>
  public static class AdminHelper
  {
    /// <summary>
    /// 현재 프로세스가 관리자 권한으로 실행 중인지 확인합니다.
    /// </summary>
    /// <returns>관리자 권한으로 실행 중이면 true, 아니면 false</returns>
    public static bool IsRunningAsAdmin()
    {
      using (var identity = WindowsIdentity.GetCurrent())
      {
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
      }
    }

    /// <summary>
    /// 관리자 권한이 없으면 관리자 권한으로 애플리케이션을 재실행합니다.
    /// </summary>
    public static void RequireAdminOrExit()
    {
      if (IsRunningAsAdmin())
        return;

      RestartAsAdmin();
    }

    /// <summary>
    /// 관리자 권한으로 애플리케이션을 재실행합니다.
    /// </summary>
    private static void RestartAsAdmin()
    {
      var processInfo = new ProcessStartInfo
      {
        FileName = Process.GetCurrentProcess().MainModule?.FileName,
        UseShellExecute = true,
        Verb =
          "runas" // 관리자 권한으로 실행
        ,
      };

      try
      {
        Process.Start(processInfo);
        Environment.Exit(0);
      }
      catch
      {
        Environment.Exit(1);
      }
    }
  }
}
