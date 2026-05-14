using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace SpeechAgent.Utils
{
  public enum LogLevel
  {
    Debug,
    Info,
    Warn,
    Error,
  }

  public static class LogUtils
  {
    private static readonly object _fileLock = new();
    private static readonly object _initLock = new();
    private static readonly int _pid = Environment.ProcessId;
    private static string? _resolvedLogDirectory;
    private static bool _fallbackNotified;
    private static DateTime _lastCleanupDate = DateTime.MinValue;

    private const int LogRetentionDays = 14;

    // exe 폴더가 보호된 위치(Program Files 등)이고 권한 재실행 이전이면 쓰기 실패할 수 있으므로,
    // %LocalAppData%\<projectName>\Log 를 폴백으로 시도합니다.
    private static string? GetOrInitLogDirectory()
    {
      if (_resolvedLogDirectory != null)
        return _resolvedLogDirectory;

      lock (_initLock)
      {
        if (_resolvedLogDirectory != null)
          return _resolvedLogDirectory;

        var primary = Path.Combine(PathUtils.GetExeDirectory(), "Log");
        var fallback = Path.Combine(PathUtils.GetLocalAppDataDirectory(), "Log");

        foreach (var path in new[] { primary, fallback })
        {
          if (TryUseDirectory(path))
          {
            _resolvedLogDirectory = path;
            if (path == fallback && !_fallbackNotified)
            {
              _fallbackNotified = true;
              // 폴백이 채택된 사실 자체를 기록 (재귀 호출 안전: _resolvedLogDirectory가 이미 세팅됨)
              try
              {
                string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                string filePath = Path.Combine(path, fileName);
                string line = $"[Warn] {DateTime.Now:HH:mm:ss.fff} P{_pid}/T{Environment.CurrentManagedThreadId} " +
                  $"기본 로그 경로({primary})에 쓸 수 없어 폴백 경로 사용 중";
                File.AppendAllText(filePath, line + Environment.NewLine, System.Text.Encoding.UTF8);
              }
              catch { }
            }
            return path;
          }
        }

        return null;
      }
    }

    private static bool TryUseDirectory(string path)
    {
      try
      {
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);

        // 실제 쓰기 권한 확인 — 폴더 생성만 되고 파일 쓰기가 막히는 케이스 방어
        var probePath = Path.Combine(path, $".probe-{_pid}");
        File.WriteAllText(probePath, string.Empty);
        File.Delete(probePath);
        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// 로그를 기록합니다. 파일명은 yyyy-MM-dd.txt로 자동 생성됩니다.
    /// 로그 형식: [Level] HH:mm:ss.fff PID/TID 텍스트
    /// 2주 이상 지난 로그는 자동으로 삭제됩니다.
    /// </summary>
    public static void WriteLog(LogLevel level, string text)
    {
      WriteLog(level, text, null);
    }

    /// <summary>
    /// 예외 정보를 포함하여 로그를 기록합니다. 스택트레이스가 함께 저장됩니다.
    /// </summary>
    public static void WriteLog(LogLevel level, string text, Exception? ex)
    {
      try
      {
        var logDir = GetOrInitLogDirectory();
        if (logDir == null)
          return;

        string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
        string filePath = Path.Combine(logDir, fileName);

        int tid = Environment.CurrentManagedThreadId;
        string logLine = $"[{level}] {DateTime.Now:HH:mm:ss.fff} P{_pid}/T{tid} {text}";

        if (ex != null)
        {
          logLine += Environment.NewLine + "    " + ex.GetType().FullName + ": " + ex.Message;
          if (!string.IsNullOrEmpty(ex.StackTrace))
            logLine += Environment.NewLine + ex.StackTrace;
          var inner = ex.InnerException;
          while (inner != null)
          {
            logLine += Environment.NewLine + "  --> " + inner.GetType().FullName + ": " + inner.Message;
            if (!string.IsNullOrEmpty(inner.StackTrace))
              logLine += Environment.NewLine + inner.StackTrace;
            inner = inner.InnerException;
          }
        }

        Debug.WriteLine(logLine);

        lock (_fileLock)
        {
          File.AppendAllText(filePath, logLine + Environment.NewLine, System.Text.Encoding.UTF8);
        }

        DeleteOldLogs();
      }
      catch
      {
        // 로그 저장 실패는 무시
      }
    }

    /// <summary>
    /// 새 세션의 시작을 구분하는 배너를 기록합니다. 프로세스 시작 직후 1회 호출하세요.
    /// </summary>
    public static void WriteSessionBanner(string? extra = null)
    {
      try
      {
        string banner =
          "================ SESSION START ================" + Environment.NewLine +
          $"  Time     : {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}" + Environment.NewLine +
          $"  PID      : {_pid}" + Environment.NewLine +
          $"  ProcName : {Process.GetCurrentProcess().ProcessName}" + Environment.NewLine +
          $"  BaseDir  : {AppDomain.CurrentDomain.BaseDirectory}" + Environment.NewLine +
          $"  Cmdline  : {Environment.CommandLine}" + Environment.NewLine +
          $"  OS       : {Environment.OSVersion} ({(Environment.Is64BitProcess ? "x64" : "x86")} proc)" + Environment.NewLine +
          $"  CLR      : {Environment.Version}" + Environment.NewLine +
          $"  User     : {Environment.UserDomainName}\\{Environment.UserName} (IsAdmin={AdminHelper.IsRunningAsAdmin()})";

        if (!string.IsNullOrEmpty(extra))
          banner += Environment.NewLine + "  Extra    : " + extra;

        banner += Environment.NewLine + "===============================================";

        var logDir = GetOrInitLogDirectory();
        if (logDir == null)
          return;

        string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
        string filePath = Path.Combine(logDir, fileName);

        lock (_fileLock)
        {
          File.AppendAllText(filePath, banner + Environment.NewLine, System.Text.Encoding.UTF8);
        }
      }
      catch
      {
        // 무시
      }
    }

    /// <summary>
    /// 보관기간(LogRetentionDays)이 지난 로그 파일을 삭제합니다.
    /// 파일명(yyyy-MM-dd.txt)을 기준으로 판단하여, 시스템 시간/생성시각 변경의 영향을 받지 않습니다.
    /// 매 WriteLog마다 호출되므로 하루 1회로 가드합니다.
    /// </summary>
    private static void DeleteOldLogs()
    {
      try
      {
        var today = DateTime.Today;
        if (_lastCleanupDate == today)
          return;
        _lastCleanupDate = today;

        var logDir = _resolvedLogDirectory;
        if (logDir == null || !Directory.Exists(logDir))
          return;

        var logFiles = Directory.GetFiles(logDir, "*.txt");
        var cutoffDate = DateTime.Today.AddDays(-LogRetentionDays);

        foreach (var file in logFiles)
        {
          try
          {
            var nameOnly = Path.GetFileNameWithoutExtension(file);
            // yyyy-MM-dd 형식 파일명 우선 — 파싱 실패 시 생성시각으로 폴백
            if (DateTime.TryParseExact(nameOnly, "yyyy-MM-dd",
                  System.Globalization.CultureInfo.InvariantCulture,
                  System.Globalization.DateTimeStyles.None, out var fileDate))
            {
              if (fileDate < cutoffDate)
                File.Delete(file);
            }
            else
            {
              var fileInfo = new FileInfo(file);
              if (fileInfo.CreationTime < cutoffDate)
                File.Delete(file);
            }
          }
          catch
          {
            // 개별 파일 삭제 실패는 다음 파일로 진행
          }
        }
      }
      catch
      {
        // 무시
      }
    }

    [Obsolete("WriteLog(LogLevel, string)를 사용하세요.")]
    public static void WriteTextLog(string filePath, string text, bool append = true)
    {
      try
      {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
          Directory.CreateDirectory(dir);
        string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}";
        if (append)
          File.AppendAllText(filePath, logLine + Environment.NewLine, System.Text.Encoding.UTF8);
        else
          File.WriteAllText(filePath, logLine + Environment.NewLine, System.Text.Encoding.UTF8);
      }
      catch
      {
        // 무시
      }
    }
  }
}
