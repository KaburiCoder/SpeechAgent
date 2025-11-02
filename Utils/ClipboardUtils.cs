using System.Runtime.InteropServices;
using static Vanara.PInvoke.User32;

namespace SpeechAgent.Utils
{
  public static class ClipboardUtils
  {
    public static async Task PasteTextAtCursor(this string text, int delayMs = 100)
    {
      // 0. 모든 모디파이어 키 해제
      ReleaseModifierKeys();
      await Task.Delay(50); // 안정화 대기

      // 1. 기존 클립보드 백업
      string originalClipboard = "";
      bool hasOriginal = false;
      try
      {
        if (Clipboard.ContainsText())
        {
          originalClipboard = Clipboard.GetText();
          hasOriginal = true;
        }
      }
      catch
      { /* 무시 */
      }

      try
      {
        // 2. 새 텍스트 넣기
        Clipboard.SetText(text);
        await Task.Delay(delayMs); // 클립보드 안정화

        // 3. Ctrl + V
        SendKeys.SendWait("^v");
      }
      finally
      {
        // 4. 클립보드 복원 (필요시)
        if (hasOriginal)
        {
          try
          {
            Clipboard.SetText(originalClipboard);
          }
          catch { }
        }
      }
    }

    private static void ReleaseModifierKeys()
    {
      // Vanara를 사용하여 모든 모디파이어 키 해제
      var inputs = new INPUT[]
      {
        CreateKeyUpInput(VK.VK_CONTROL),
        CreateKeyUpInput(VK.VK_MENU), // Alt
        CreateKeyUpInput(VK.VK_SHIFT),
        CreateKeyUpInput(VK.VK_LWIN),
        CreateKeyUpInput(VK.VK_RWIN),
      };

      SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>());
    }

    private static INPUT CreateKeyUpInput(VK virtualKey)
    {
      return new INPUT
      {
        type = INPUTTYPE.INPUT_KEYBOARD,
        ki = new KEYBDINPUT { wVk = (ushort)virtualKey, dwFlags = KEYEVENTF.KEYEVENTF_KEYUP },
      };
    }
  }
}
