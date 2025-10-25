using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using SpeechAgent.Models;

namespace SpeechAgent.Features.Settings.FindWin.Models
{
  public class WindowInfo
  {
    public IntPtr Handle { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ProcessName { get; set; } = string.Empty;
    public int ProcessId { get; set; }
    public BitmapSource? Screenshot { get; set; }
    public bool IsVisible { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public ObservableCollection<ControlInfo> Controls { get; set; } = new();
  }
}
