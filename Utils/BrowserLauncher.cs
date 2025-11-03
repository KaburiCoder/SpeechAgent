using System.Diagnostics;
using System.IO;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// 브라우저를 실행하는 유틸리티 클래스
  /// Chrome -> Edge -> 기본 브라우저 순서로 시도합니다.
  /// </summary>
  public static class BrowserLauncher
  {
    private const string ChromeBrowserName = "chrome";
    private const string EdgeBrowserName = "msedge";

    public static void OpenMedic()
    {
      OpenUrl("https://medic.clickcns.com");
    }

    /// <summary>
    /// URL을 Chrome으로 먼저 시도하고, 실패 시 Edge, 그 다음 기본 브라우저로 엽니다.
    /// </summary>
    /// <param name="url">열 URL</param>
    /// <returns>성공 여부</returns>
    public static bool OpenUrl(string url)
    {
      if (string.IsNullOrWhiteSpace(url))
        return false;

      // Chrome 우선 실행
      if (TryOpenBrowser(url, ChromeBrowserName))
        return true;

      // Chrome 실패 시 Edge 실행
      if (TryOpenBrowser(url, EdgeBrowserName))
        return true;

      // Edge도 실패 시 기본 브라우저 실행
      return TryOpenBrowser(url, null);
    }

    /// <summary>
    /// 특정 브라우저로 URL을 엽니다.
    /// </summary>
    /// <param name="url">열 URL</param>
    /// <param name="browserName">브라우저 이름 (chrome, msedge, null: 기본 브라우저)</param>
    /// <returns>성공 여부</returns>
    private static bool TryOpenBrowser(string url, string? browserName)
    {
      try
      {
        if (browserName == null)
        {
          // 기본 브라우저로 열기
          Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
          return true;
        }

        // 특정 브라우저 경로 찾기
        string? browserPath = FindBrowserPath(browserName);

        if (string.IsNullOrEmpty(browserPath))
          return false;

        // 브라우저 실행
        Process.Start(
          new ProcessStartInfo
          {
            FileName = browserPath,
            Arguments = url,
            UseShellExecute = false,
          }
        );
        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// 시스템에 설치된 브라우저의 경로를 찾습니다.
    /// </summary>
    /// <param name="browserName">브라우저 이름 (chrome, msedge)</param>
    /// <returns>브라우저 실행 파일 경로, 없으면 null</returns>
    private static string? FindBrowserPath(string browserName)
    {
      var commonPaths = browserName switch
      {
        ChromeBrowserName =>
        [
          @"C:\Program Files\Google\Chrome\Application\chrome.exe",
          @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
          Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            @"Google\Chrome\Application\chrome.exe"
          ),
        ],
        EdgeBrowserName =>
        [
          @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
          @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
          Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Microsoft\Edge\Application\msedge.exe"
          ),
        ],
        _ => Array.Empty<string>(),
      };

      return commonPaths.FirstOrDefault(path => File.Exists(path));
    }
  }
}
