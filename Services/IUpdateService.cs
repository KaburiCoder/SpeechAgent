using System;
using System.Threading.Tasks;

namespace SpeechAgent.Services
{
  public interface IUpdateService
  {
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;
    event EventHandler<UpdateErrorEventArgs>? UpdateError;
    
    void StartPeriodicCheck();
    void StopPeriodicCheck();
    Task CheckForUpdatesAsync();
  }

  public class UpdateAvailableEventArgs : EventArgs
  {
    public string NewVersion { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
  }

  public class UpdateErrorEventArgs : EventArgs
  {
    public Exception Exception { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
  }
}