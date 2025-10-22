using System.Windows.Threading;
using Velopack;

namespace SpeechAgent.Services
{
  public interface IUpdateService
  { 
    event EventHandler<UpdateErrorEventArgs>? UpdateError;

    void StartPeriodicCheck();
    void StopPeriodicCheck();
    Task CheckForUpdatesAsync();
  }

  public class UpdateErrorEventArgs : EventArgs
  {
    public Exception Exception { get; set; } = default!;
    public string Message { get; set; } = string.Empty;
  }

  public class UpdateService : IUpdateService
  {
    private readonly DispatcherTimer _timer;
    private readonly string _updateUrl;
     
    public event EventHandler<UpdateErrorEventArgs>? UpdateError;

    public UpdateService()
    {
      _updateUrl = "https://github.com/KaburiCoder/SpeechAgent/releases/latest/download";
      _timer = new DispatcherTimer
      {
        Interval = TimeSpan.FromMinutes(3) // 3분마다 체크
      };
      _timer.Tick += async (s, e) => await CheckForUpdatesAsync();
    }

    public void StartPeriodicCheck()
    {
      _timer.Start();
    }

    public void StopPeriodicCheck()
    {
      _timer.Stop();
    }

    public async Task CheckForUpdatesAsync()
    {
#if !DEBUG
      try
      {
        var mgr = new UpdateManager(_updateUrl);
        var newVersion = await mgr.CheckForUpdatesAsync();
        
        if (newVersion == null)
          return; // no update available              

        // 자동으로 다운로드 및 설치
        await mgr.DownloadUpdatesAsync(newVersion);
        mgr.ApplyUpdatesAndRestart(newVersion);
      }
      catch (Exception ex)
      {
        UpdateError?.Invoke(this, new UpdateErrorEventArgs 
        { 
          Exception = ex, 
          Message = ex.Message 
        });
      }
#else
      await Task.CompletedTask;
#endif
    }
  }
}