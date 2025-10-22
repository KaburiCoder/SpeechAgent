using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Velopack;
using Velopack.Sources;

namespace SpeechAgent.Services
{
  public class UpdateService : IUpdateService
  {
    private readonly DispatcherTimer _timer;
    private readonly string _updateUrl;

    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;
    public event EventHandler<UpdateErrorEventArgs>? UpdateError;

    public UpdateService()
    {
      _updateUrl = "https://github.com/KaburiCoder/SpeechAgent/releases/latest/download";
      _timer = new DispatcherTimer
      {
        Interval = TimeSpan.FromMinutes(1) // 1�и��� üũ
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

        // ������Ʈ�� ���� �� �̺�Ʈ �߻�
        UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs 
        { 
          NewVersion = newVersion.TargetFullRelease?.Version?.ToString() ?? "Unknown",
          DownloadUrl = _updateUrl
        });

        // �ڵ����� �ٿ�ε� �� ��ġ
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