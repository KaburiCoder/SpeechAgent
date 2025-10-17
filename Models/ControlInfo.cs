using Vanara.PInvoke;

namespace SpeechAgent.Models
{
  public class ControlInfo
  {
    public HWND Hwnd { get; set; }
    public RECT RECT { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
  }
}