using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vanara.PInvoke;

namespace SpeechAgent.Utils
{
  public static class WinAPIUtils
  {
    public static HWND FindWindowByPredicate(Func<string, bool> predicate)
    {
      HWND result = IntPtr.Zero;
      User32.EnumWindows((hWnd, lParam) =>
      {
        if (!User32.IsWindowVisible(hWnd)) return true;
        int length = User32.GetWindowTextLength(hWnd);
        if (length == 0) return true;
        var sb = new StringBuilder(length + 1);
        User32.GetWindowText(hWnd, sb, sb.Capacity);
        string title = sb.ToString();
        if (predicate(title))
        {
          result = hWnd;
          return false; // 찾았으니 중단
        }
        return true;
      }, IntPtr.Zero);
      return result;
    }
  }
}
