using System.Diagnostics;
using System.Windows.Threading;
using SpeechAgent.Constants;
using SpeechAgent.Utils;
using Velopack;

namespace SpeechAgent.Services
{
  public interface IUpdateService
  {
    event EventHandler<UpdateErrorEventArgs>? UpdateError;

    void StartPeriodicCheck();
    void StopPeriodicCheck();
    Task CheckForUpdatesAsync();
    bool IsUpdateApplied { get; set; }
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
    private const string UpdateAppliedFlag = "SPEECH_AGENT_UPDATE_APPLIED";

    public bool IsUpdateApplied
    {
      get =>
        Environment.GetEnvironmentVariable(UpdateAppliedFlag, EnvironmentVariableTarget.User)
        == "1";
      set
      {
        if (value)
          Environment.SetEnvironmentVariable(
            UpdateAppliedFlag,
            "1",
            EnvironmentVariableTarget.User
          );
        else
          Environment.SetEnvironmentVariable(
            UpdateAppliedFlag,
            null,
            EnvironmentVariableTarget.User
          );
      }
    }

    public event EventHandler<UpdateErrorEventArgs>? UpdateError;

    public UpdateService()
    {
      // UpdateConfig에서 URL을 불러옴
      _updateUrl = Environment.Is64BitProcess
        ? UpdateConfig.UpdateUrlX64
        : UpdateConfig.UpdateUrlX86;

      _timer = new DispatcherTimer
      {
        Interval = TimeSpan.FromMinutes(
          3
        ) // 3분마다 체크
        ,
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

        // 업데이트 적용 플래그 설정
        IsUpdateApplied = true;

        mgr.ApplyUpdatesAndRestart(newVersion);
      }
      catch (Exception ex)
      {
        UpdateError?.Invoke(
          this,
          new UpdateErrorEventArgs { Exception = ex, Message = ex.Message }
        );
      }
#else
      await Task.CompletedTask;
#endif
    }
  }
}
