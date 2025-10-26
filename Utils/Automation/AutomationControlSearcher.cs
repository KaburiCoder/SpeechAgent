using System.Windows.Automation;
using SpeechAgent.Models;

namespace SpeechAgent.Utils.Automation
{
  public class AutomationControlSearcher
  {
    private AutomationElement? _targetWindow;
    private readonly List<AutomationControlInfo> _foundControls = new();
    private readonly AutomationElementCollector _collector = new();

    public List<AutomationControlInfo> FoundControls => _foundControls;

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

      _foundControls.Sort((a, b) =>
      {
        int topComparison = a.BoundingRectangle.Left.CompareTo(b.BoundingRectangle.Left);
        if (topComparison != 0)
          return topComparison;
        return a.BoundingRectangle.Top.CompareTo(b.BoundingRectangle.Top);
      });

      return _foundControls;
    }

    private AutomationControlInfo? CreateControlInfo(AutomationElement element)
    {
      try
      {
        var rect = element.Current.BoundingRectangle;
        var className = element.Current.ClassName;
        var name = element.Current.Name;
        var automationId = element.Current.AutomationId;
        var controlType = element.Current.ControlType.ProgrammaticName;

        // �ؽ�Ʈ �������� �õ�
        string text = name;
        if (string.IsNullOrEmpty(text))
        {
          try
          {
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out object? valuePattern))
            {
              text = ((ValuePattern)valuePattern).Current.Value;
            }
          }
          catch
          {
            // ����
          }
        }

        return new AutomationControlInfo
        {
          Element = element,
          ClassName = className ?? "",
          Name = name ?? "",
          AutomationId = automationId ?? "",
          ControlType = controlType ?? "",
          Text = text ?? "",
          BoundingRectangle = new System.Drawing.Rectangle(
                (int)rect.Left,
               (int)rect.Top,
             (int)rect.Width,
                (int)rect.Height)
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
  }
}
