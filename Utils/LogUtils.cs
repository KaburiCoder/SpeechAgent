using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SpeechAgent.Utils
{
  /// <summary>
  /// 로그 레벨 열거형
  /// </summary>
  public enum LogLevel
  {
    Debug,
    Info,
    Error,
  }

  public static class LogUtils
  {
    private static readonly string LogDirectory = Path.Combine(
      AppDomain.CurrentDomain.BaseDirectory,
      "Log"
    );

    /// <summary>
    /// 로그를 저장합니다. 파일명은 yyyy-MM-dd.txt로 자동 생성됩니다.
    /// 로그 형식: [LogLevel] HH:mm:ss 텍스트
    /// 3일 이상 지난 로그는 자동으로 삭제됩니다.
    /// </summary>
    /// <param name="level">로그 레벨 (Debug, Info, Error)</param>
    /// <param name="text">저장할 텍스트</param>
    public static void WriteLog(LogLevel level, string text)
    {
      try
      {
        // Log 폴더 생성
        if (!Directory.Exists(LogDirectory))
          Directory.CreateDirectory(LogDirectory);

        // 파일경로: Log/yyyy-MM-dd.txt
        string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
        string filePath = Path.Combine(LogDirectory, fileName);

        // 로그 라인 형식: [Level] HH:mm:ss 텍스트
        string logLine = $"[{level}] {DateTime.Now:HH:mm:ss} {text}";

        Debug.WriteLine(logLine);

        // 파일에 추가
        File.AppendAllText(filePath, logLine + Environment.NewLine, System.Text.Encoding.UTF8);

        // 3일 이상 지난 로그 삭제
        DeleteOldLogs();
      }
      catch
      {
        // 로그 저장 실패는 무시
      }
    }

    /// <summary>
    /// 3일 이상 지난 로그 파일을 삭제합니다.
    /// </summary>
    private static void DeleteOldLogs()
    {
      try
      {
        if (!Directory.Exists(LogDirectory))
          return;

        var logFiles = Directory.GetFiles(LogDirectory, "*.txt");
        var cutoffDate = DateTime.Now.AddDays(-3);

        foreach (var file in logFiles)
        {
          var fileInfo = new FileInfo(file);
          if (fileInfo.CreationTime < cutoffDate)
          {
            File.Delete(file);
          }
        }
      }
      catch
      {
        // 로그 삭제 실패는 무시
      }
    }

    /// <summary>
    /// 텍스트 파일로 로그를 저장합니다. (기본적으로 UTF-8 인코딩)
    /// 로그에 시간 정보가 자동으로 추가됩니다.
    /// </summary>
    /// <param name="filePath">저장할 파일 경로</param>
    /// <param name="text">저장할 텍스트</param>
    /// <param name="append">true면 기존 파일에 추가, false면 새로 작성</param>
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
        // 로그 저장 실패는 무시
      }
    }
  }
}
