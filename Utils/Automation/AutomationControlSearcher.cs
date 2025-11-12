using System.Windows.Automation;
using SpeechAgent.Models;

namespace SpeechAgent.Utils.Automation
{
  public interface IAutomationControlSearcher
  {
    List<AutomationControlInfo> FoundControls { get; }
    AutomationControlInfo? CreateControlInfo(AutomationElement? element);
    bool FindWindowByTitle(Func<string, bool> titlePredicate);
    bool FindWindowByTitles(params string[] titleSubstrings);
    bool FindWindowByHandle(IntPtr handle);
    List<AutomationControlInfo> SearchControls();
    string GetControlText(AutomationElement element);
    bool IsWindowValid();
    IntPtr GetWindowHandle();
    void ClearFoundControls();
  }

  public class AutomationControlSearcher : IAutomationControlSearcher
  {
    private AutomationElement? _targetWindow;
    private readonly List<AutomationControlInfo> _foundControls = new();
    private readonly AutomationElementCollector _collector = new();

    public List<AutomationControlInfo> FoundControls => _foundControls;

    public bool FindWindowByTitles(params string[] titleSubstrings)
    {
      return FindWindowByTitle(title => titleSubstrings.All(sub => title.Contains(sub)));
    }

    public bool FindWindowByTitle(Func<string, bool> titlePredicate)
    {
      try
      {
        var windows = _collector.GetAllWindows();
        _targetWindow = windows.FirstOrDefault(w =>
        {
          try
          {
            return titlePredicate(w.Current.Name);
          }
          catch
          {
            return false;
          }
        });

        return _targetWindow != null;
      }
      catch
      {
        return false;
      }
    }

    public bool FindWindowByHandle(IntPtr handle)
    {
      try
      {
        _targetWindow = _collector.GetElementByHandle(handle);
        return _targetWindow != null;
      }
      catch
      {
        return false;
      }
    }

    public List<AutomationControlInfo> SearchControls()
    {
      _foundControls.Clear();

      if (_targetWindow == null)
        return _foundControls;

      _collector.CollectElements(_targetWindow);
      var elements = _collector.GetElements();

      foreach (var element in elements)
      {
        try
        {
          var controlInfo = CreateControlInfo(element);
          if (controlInfo != null)
          {
            _foundControls.Add(controlInfo);
          }
        }
        catch (ElementNotAvailableException)
        {
          // 무시
        }
      }

      // 컨트롤 타입별로 그룹화하여 Index 재설정
      var grouped = _foundControls.GroupBy(c => c.ControlType);
      foreach (var group in grouped)
      {
        int index = 0;
        foreach (var control in group)
        {
          control.Index = index++;
        }
      }

      _foundControls.Sort(
        (a, b) =>
        {
          int topComparison = a.BoundingRectangle.Left.CompareTo(b.BoundingRectangle.Left);
          if (topComparison != 0)
            return topComparison;
          return a.BoundingRectangle.Top.CompareTo(b.BoundingRectangle.Top);
        }
      );

      return _foundControls;
    }

    public AutomationControlInfo? CreateControlInfo(AutomationElement? element)
    {
      if (element == null)
        return null;

      try
      {
        var rect = element.Current.BoundingRectangle;
        var className = element.Current.ClassName;
        var name = element.Current.Name;
        var automationId = element.Current.AutomationId;
        var controlType = element.Current.ControlType.ProgrammaticName;

        // 텍스트 가져오기 시도
        string text = GetControlText(element);

        return new AutomationControlInfo
        {
          Element = element,
          ClassName = className ?? "",
          Name = name ?? "",
          AutomationId = automationId ?? "",
          ControlType = controlType ?? "",
          Text = text ?? "",
          BoundingRectangle = new Rectangle(
            (int)rect.Left,
            (int)rect.Top,
            (int)rect.Width,
            (int)rect.Height
          ),
        };
      }
      catch
      {
        return null;
      }
    }

    public string GetControlText(AutomationElement element)
    {
      try
      {
        // Name 속성 먼저 확인
        var name = element.Current.Name;
        if (!string.IsNullOrEmpty(name))
          return name;

        // ValuePattern 시도
        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
        {
          return ((ValuePattern)valuePattern).Current.Value ?? "";
        }

        // TextPattern 시도
        if (element.TryGetCurrentPattern(TextPattern.Pattern, out object? textPattern))
        {
          return ((TextPattern)textPattern).DocumentRange.GetText(-1) ?? "";
        }

        return "";
      }
      catch
      {
        return "";
      }
    }

    public bool IsWindowValid()
    {
      if (_targetWindow == null)
        return false;

      try
      {
        // BoundingRectangle 접근 시도로 유효성 확인
        _ = _targetWindow.Current.BoundingRectangle;
        return true;
      }
      catch
      {
        return false;
      }
    }

    public void ClearFoundControls()
    {
      _foundControls.Clear();
      _targetWindow = null;
    }

    public IntPtr GetWindowHandle()
    {
      if (_targetWindow == null)
        return 0;

      try
      {
        return _targetWindow.Current.NativeWindowHandle;
      }
      catch
      {
        return 0;
      }
    }
  }
}
