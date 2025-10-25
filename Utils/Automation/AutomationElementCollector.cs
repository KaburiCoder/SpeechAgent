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
 // ����
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
        // ����
  }
    }

    public List<AutomationElement> GetElements()
    {
      return _elements
        .OrderBy(e => e.Current.BoundingRectangle.Left)
        .ThenBy(e => e.Current.BoundingRectangle.Top)
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
          // ����
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
          // �� â�̳� ������ â ����
          if (!string.IsNullOrEmpty(window.Current.Name) && 
    window.Current.BoundingRectangle.Width > 0)
          {
 windows.Add(window);
          }
        }
    catch (ElementNotAvailableException)
        {
          // ����
        }
      }

      return windows;
 }
  }
}
