using System;
using System.IO;

namespace SpeechAgent.Utils
{
  public static class LogUtils
  {
    /// <summary>
    /// �ؽ�Ʈ ���Ϸ� �α׸� �����մϴ�. (�⺻������ UTF-8 ���ڵ�)
    /// �α׿� �ð� ������ �ڵ����� �߰��˴ϴ�.
    /// </summary>
    /// <param name="filePath">������ ���� ���</param>
    /// <param name="text">������ �ؽ�Ʈ</param>
    /// <param name="append">true�� ���� ���Ͽ� �߰�, false�� ���� �ۼ�</param>
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
        // �α� ���� ���д� ����
      }
    }
  }
}
