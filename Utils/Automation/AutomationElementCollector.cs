using System.Diagnostics;
using System.Windows.Automation;

namespace SpeechAgent.Utils.Automation
{
  public class AutomationElementCollector
  {
    private readonly List<AutomationElement> _elements = new();

    public void CollectElements(string windowTitle)
    {
      _elements.Clear();
      var targetWindow = FindWindow(windowTitle);
      if (targetWindow != null)
      {
        EnumerateElements(targetWindow);
      }
    }

    public void CollectElements(AutomationElement window)
    {
      _elements.Clear();
      EnumerateElements(window);
    }

    private AutomationElement? FindWindow(string windowTitle)
    {
      var rootElement = AutomationElement.RootElement;
      AutomationElement? targetWindow = null;

      var windows = rootElement.FindAll(
        TreeScope.Children,
  new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

      foreach (AutomationElement window in windows)
      {
        try
        {
          if (window.Current.Name.Contains(windowTitle))
          {
            targetWindow = window;
            break;
          }
        }
        catch (ElementNotAvailableException)
        {
          // 무시
        }
      }

      return targetWindow;
    }

    private void EnumerateElements(AutomationElement element)
    {
      try
      {
        _elements.Add(element);

        TreeWalker walker = TreeWalker.RawViewWalker;
        AutomationElement? child = walker.GetFirstChild(element);

        while (child != null)
        {
          EnumerateElements(child);
          child = walker.GetNextSibling(child);
        }
      }
      catch (ElementNotAvailableException)
      {
        // 무시
      }
    }

    public List<AutomationElement> GetElements()
    {
      var validElements = new List<AutomationElement>();

      // 먼저 유효한 요소만 필터링
      foreach (var element in _elements)
      {
        try
        {
          // BoundingRectangle 접근 시도 - 유효성 검사
          var rect = element.Current.BoundingRectangle;
          validElements.Add(element);
        }
        catch (ElementNotAvailableException)
        {
          // 요소가 더 이상 유효하지 않으면 스킵
        }
        catch (InvalidOperationException)
        {
          // 요소의 상태가 변했으면 스킵
        }
      }

      // 유효한 요소들만 정렬
      return validElements
         .OrderBy(e =>
           {
             try
             {
               return e.Current.BoundingRectangle.Left;
             }
             catch
             {
               return int.MaxValue; // 오류 발생 시 끝에 배치
             }
           })
           .ThenBy(e =>
           {
             try
             {
               return e.Current.BoundingRectangle.Top;
             }
             catch
             {
               return int.MaxValue;
             }
           })
           .ToList();
    }

    public AutomationElement? GetElementByHandle(IntPtr handle)
    {
      try
      {
        return AutomationElement.FromHandle(handle);
      }
      catch
      {
        return null;
      }
    }

    public List<AutomationElement> GetWindows(string windowTitle)
    {
      var rootElement = AutomationElement.RootElement;
      var windows = new List<AutomationElement>();

      var allWindows = rootElement.FindAll(
        TreeScope.Children,
        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

      foreach (AutomationElement window in allWindows)
      {
        try
        {
          if (window.Current.Name.Contains(windowTitle))
          {
            windows.Add(window);
          }
        }
        catch (ElementNotAvailableException)
        {
          // 무시
        }
      }

      return windows;
    }

    public List<AutomationElement> GetAllWindows()
    {
      var rootElement = AutomationElement.RootElement;
      var windows = new List<AutomationElement>();

      var allWindows = rootElement.FindAll(
        TreeScope.Children,
        new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));

      foreach (AutomationElement window in allWindows)
      {
        try
        {
          // 빈 창이나 숨겨진 창 제외
          if (!string.IsNullOrEmpty(window.Current.Name) &&
            window.Current.BoundingRectangle.Width > 0)
          {
            windows.Add(window);
          }
        }
        catch (ElementNotAvailableException)
        {
          // 무시
        }
      }

      return windows;
    }
  }
}
