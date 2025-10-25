using System.Windows.Automation;

namespace SpeechAgent.Models
{
  public class AutomationControlInfo
  {
    public AutomationElement Element { get; set; } = null!;
    public string ClassName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AutomationId { get; set; } = string.Empty;
    public string ControlType { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public System.Drawing.Rectangle BoundingRectangle { get; set; }
    public int Index { get; set; }

    // WPF 바인딩을 위한 속성들
    public int RectLeft => BoundingRectangle.Left;
    public int RectTop => BoundingRectangle.Top;
    public int RectWidth => BoundingRectangle.Width;
    public int RectHeight => BoundingRectangle.Height;

    public string DisplayText => $"[{Index}] Class: {ClassName}\nAutomationId: {AutomationId}\nControlType: {ControlType}\nPosition: x:{RectLeft}, y:{RectTop}, w:{RectWidth}, h:{RectHeight}";
  }
}
