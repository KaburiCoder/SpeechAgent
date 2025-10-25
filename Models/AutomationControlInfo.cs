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
  }
}
