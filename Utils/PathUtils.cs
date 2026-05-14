using System;
using System.IO;
using System.Reflection;

namespace SpeechAgent.Utils
{
  public static class PathUtils
  {
    /// <summary>
    /// 실행 중인 exe 파일이 있는 디렉토리를 반환합니다.
    /// 단일 파일(self-extract) 빌드에서는 AppDomain.BaseDirectory가 임시 추출 폴더를 가리키므로
    /// Environment.ProcessPath 기준으로 산출합니다.
    /// </summary>
    public static string GetExeDirectory()
    {
      return Path.GetDirectoryName(Environment.ProcessPath)
        ?? AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// %LocalAppData%\&lt;projectName&gt; 디렉토리 경로를 반환합니다.
    /// projectName은 EntryAssembly 이름이며, 실패 시 "SpeechAgent"로 폴백합니다.
    /// </summary>
    public static string GetLocalAppDataDirectory()
    {
      var projectName = Assembly.GetEntryAssembly()?.GetName().Name ?? "SpeechAgent";
      return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        projectName);
    }
  }
}
