using SpeechAgent.Models;
using System.Text;
using Vanara.PInvoke;

namespace SpeechAgent.Utils
{
  public interface IControlSearcher
  {
    List<ControlInfo> SearchControls();
    bool FindWindowByTitle(Func<string, bool> winTitlePredicate);
    bool IsHwndValid();
    void ClearFoundControls();
    void SetHwnd(HWND hwnd);
    string GetControlText(HWND hwnd);
    List<ControlInfo> FoundControls { get; }
  }

  /// <summary>
  /// Win32 API를 사용하여 윈도우 컨트롤을 검색하는 클래스
  /// </summary>
  public class ControlSearcher : IControlSearcher
  {
    private readonly Func<string, bool>? _classNamePredicate;
    private readonly List<ControlInfo> _foundControls = new();
    private readonly StringBuilder _classNameSb = new(256);
    private HWND _hwnd;

    private bool EnumChildProc(HWND hwnd, nint lParam)
    {
      _classNameSb.Clear();
      User32.GetClassName(hwnd, _classNameSb, _classNameSb.Capacity);

      string className = _classNameSb.ToString();
      bool isMatched = _classNamePredicate?.Invoke(className) ?? true;

      if (!isMatched) return true;
      User32.GetWindowRect(hwnd, out RECT rect);

      string text = GetControlText(hwnd);
      _foundControls.Add(new ControlInfo
      {
        Hwnd = hwnd,
        ClassName = className,
        Text = text,
        RECT = rect
      });

      return true;
    }

    public ControlSearcher(
      Func<string, bool>? classNamePredicate = null)
    {
      _classNamePredicate = classNamePredicate;

      if (_hwnd == IntPtr.Zero)
        return;
    }

    public string GetControlText(HWND hwnd)
    {
      int length = (int)User32.SendMessage(hwnd, User32.WindowMessage.WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
      StringBuilder sb = new(length + 1);
      User32.SendMessage(hwnd, User32.WindowMessage.WM_GETTEXT, (IntPtr)sb.Capacity, sb);
      string text = sb.ToString();
      return text.Trim();
    }

    public void SetHwnd(HWND hwnd)
    {
      _hwnd = hwnd;
    }

    public List<ControlInfo> SearchControls()
    {
      _foundControls.Clear();
      if (_hwnd == IntPtr.Zero) return _foundControls;
      User32.EnumChildWindows(_hwnd, EnumChildProc, IntPtr.Zero);

      // 위치로 정렬
      _foundControls.Sort((a, b) =>
      {
        int leftComparison = a.RECT.Left.CompareTo(b.RECT.Left);
        if (leftComparison != 0)
          return leftComparison;
        return a.RECT.Top.CompareTo(b.RECT.Top);
      });

      // 클래스별로 인덱스 재할당
      var classNameGroups = _foundControls.GroupBy(c => c.ClassName);
      foreach (var group in classNameGroups)
      {
        int index = 0;
        foreach (var control in group)
        {
          control.Index = index++;
        }
      }

      return _foundControls;
    }

    public bool FindWindowByTitle(Func<string, bool> winTitlePredicate)
    {
      _hwnd = WinAPIUtils.FindWindowByPredicate(winTitlePredicate);
      return _hwnd != IntPtr.Zero;
    }

    public bool IsHwndValid()
    {
      return User32.IsWindow(_hwnd);
    }

    public void ClearFoundControls()
    {
      _foundControls.Clear();
    }

    public List<ControlInfo> FoundControls => _foundControls;
  }
}