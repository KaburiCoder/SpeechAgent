using System.Windows.Automation;
using SpeechAgent.Models;
using SpeechAgent.Utils;

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
    private static DateTime _lastTitleListLogTime = DateTime.MinValue;

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

        if (_targetWindow == null && (DateTime.Now - _lastTitleListLogTime).TotalSeconds > 30)
        {
          _lastTitleListLogTime = DateTime.Now;
          var titles = windows
            .Select(w =>
            {
              try
              {
                return w.Current.Name;
              }
              catch
              {
                return "<error>";
              }
            })
            .Where(t => !string.IsNullOrEmpty(t))
            .ToList();
          LogUtils.WriteLog(
            LogLevel.Debug,
            $"[FindWindowByTitle] 매칭 실패 - 현재 최상위 윈도우 {titles.Count}개: {string.Join(" | ", titles)}"
          );
        }

        return _targetWindow != null;
      }
      catch (Exception ex)
      {
        LogUtils.WriteLog(LogLevel.Error, $"[FindWindowByTitle] 예외: {ex.Message}");
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
          // ����
        }
      }

      // ��Ʈ�� Ÿ�Ժ��� �׷�ȭ�Ͽ� Index �缳��
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

        // �ؽ�Ʈ �������� �õ�
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
        // Name �Ӽ� ���� Ȯ��
        var name = element.Current.Name;
        if (!string.IsNullOrEmpty(name))
          return name;

        // ValuePattern �õ�
        if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
        {
          return ((ValuePattern)valuePattern).Current.Value ?? "";
        }

        // TextPattern �õ�
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
        // BoundingRectangle ���� �õ��� ��ȿ�� Ȯ��
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
