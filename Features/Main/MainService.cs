using System.Timers;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Constants;
using SpeechAgent.Features.Settings;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using SpeechAgent.Services.MedicSIO;
using SpeechAgent.Services.MedicSIO.Dto;

namespace SpeechAgent.Features.Main
{
  public interface IMainService
  {
    void StartReadChartTimer();
    void StopReadChartTimer();
  }

  public class MainService : IMainService
  {
    private readonly IPatientSearchService _patientSearchService;
    private readonly IMedicSIOService _medicSIOService;
    private readonly ISettingsService _settingsService;
    private readonly System.Timers.Timer _timer;
    private readonly Dispatcher _uiDispatcher;
    private PatientInfo _patientInfo = new("", "", DateTime.MinValue);
    private bool _shouldRun = false;

    public MainService(
      IPatientSearchService patientSearchService,
      IMedicSIOService medicSIOService,
      ISettingsService settingsService
    )
    {
      _patientSearchService = patientSearchService;
      _medicSIOService = medicSIOService;
      this._settingsService = settingsService;
      _timer = new System.Timers.Timer();
      _timer.AutoReset = false; // 중복 실행 방지
      _timer.Elapsed += Timer_Elapsed;
      _uiDispatcher = Dispatcher.CurrentDispatcher;

      WeakReferenceMessenger.Default.Register<LocalSettingsChangedMessage>(
        this,
        async (r, m) =>
        {
          _patientSearchService.Clear();
          bool isNoneTargetApp = string.IsNullOrWhiteSpace(m.Value.Settings.TargetAppName);
          if (isNoneTargetApp)
            StopReadChartTimer();
          else
          {
            StartReadChartTimer();
          }

          var prevKey = m.Value.PreviousSettings?.ConnectKey;
          var newKey = m.Value.Settings.ConnectKey;
          if (prevKey != newKey)
          {
            if (!string.IsNullOrWhiteSpace(prevKey))
              await _medicSIOService.LeaveRoom(prevKey);
            await _medicSIOService.JoinRoom();
          }
        }
      );
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      try
      {
        // 백그라운드 스레드에서 무거운 작업 실행 (UI Automation)
        var patientInfo = await _patientSearchService.FindPatientInfo();
        var previousPatientInfo = _patientInfo;

        // UI 스레드에서만 상태 업데이트 및 메시지 발송
        _uiDispatcher.Invoke(() =>
        {
          _patientInfo = patientInfo;

          if (!patientInfo.IsEqual(previousPatientInfo) && !patientInfo.HasOnlyOneInfo())
          {
            // 네트워크 작업은 비동기로 처리 (UI 스레드 블로킹 안 함)
            _ = SendPatientInfoAsync(_patientInfo);
          }
        });
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error in MainService timer: {ex.Message}");
      }
      finally
      {
        if (_shouldRun)
          _timer.Start();
      }
    }

    private async Task SendPatientInfoAsync(PatientInfo patientInfo)
    {
      try
      {
        await _medicSIOService.SendPatientInfo(
          new PatientInfoDto { Chart = patientInfo.Chart, Name = patientInfo.Name }
        );

        WeakReferenceMessenger.Default.Send(new PatientInfoUpdatedMessage(patientInfo));
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error sending patient info: {ex.Message}");
      }
    }

    public void StartReadChartTimer()
    {
      int intervalSec = (_settingsService.Settings.TargetAppName == AppKey.CustomUserImage) ? 3 : 1;
      _timer.Interval = intervalSec * 1000; // 밀리초 단위
      _shouldRun = true;
      _timer.Start();
    }

    public void StopReadChartTimer()
    {
      _shouldRun = false;
      _timer.Stop();
    }
  }
}
