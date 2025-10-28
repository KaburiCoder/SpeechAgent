using System;
using System.IO;

namespace SpeechAgent.Utils
{
  public static class LogUtils
  {
    /// <summary>
    /// 텍스트 파일로 로그를 저장합니다. (기본적으로 UTF-8 인코딩)
    /// 로그에 시간 정보가 자동으로 추가됩니다.
    /// </summary>
    /// <param name="filePath">저장할 파일 경로</param>
    /// <param name="text">저장할 텍스트</param>
    /// <param name="append">true면 기존 파일에 추가, false면 새로 작성</param>
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
