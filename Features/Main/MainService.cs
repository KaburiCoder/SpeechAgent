using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Features.Settings;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using SpeechAgent.Services.MedicSIO;
using SpeechAgent.Services.MedicSIO.Dto;
using SpeechAgent.Services.NamedPipe;
using System.Timers;
using System.Windows.Threading;

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
    private readonly INamedPipeService _namedPipeService;
    private readonly IViewService _viewService;
    private readonly System.Timers.Timer _timer;
    private readonly Dispatcher _uiDispatcher;
    private PatientInfo _patientInfo = new("", "", DateTime.MinValue);
    private bool _shouldRun = false;

    public MainService(
      IPatientSearchService patientSearchService,
      ISettingsService settingsService,
      IUserNotificationService userNotificationService,
      INamedPipeService namedPipeService,
      IViewService viewService
    )
    {
      _patientSearchService = patientSearchService;
      _namedPipeService = namedPipeService;
      this._viewService = viewService;
      _namedPipeService.Connected += (s, e) => OnNamedPipeConnectChanged(true);
      _namedPipeService.Disconnected += (s, e) => OnNamedPipeConnectChanged(false);
      _namedPipeService.ConnectionError += (s, e) => OnNamedPipeConnectChanged(false);
      _namedPipeService.MessageReceived += _namedPipeService_MessageReceived;
      _namedPipeService.ConnectAsync();

      _timer = new System.Timers.Timer();
      _timer.AutoReset = false;
      _timer.Elapsed += Timer_Elapsed;
      _uiDispatcher = Dispatcher.CurrentDispatcher;
      userNotificationService.StartIntervalFeedbackNotification();
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
        }
      );
    }

    private async void _namedPipeService_MessageReceived(object? sender, NamedPipeData e)
    {
      switch (e.Action)
      {
        case NamePipeReceiveAction.PING:
          await _namedPipeService.SendAsync(new NamedPipeData(NamedPipeAction.PONG, "pong"));
          break;
        case NamePipeReceiveAction.OPEN_SETTINGS:
          await _uiDispatcher.InvokeAsync(() => _viewService.ShowSettingsView());
          break;
        case NamePipeReceiveAction.GET_CURRENT_CHART:
          // voice-medic 수동 모드 — 현재 차트 정보를 즉시 응답
          await _namedPipeService.SendAsync(new NamedPipeData(NamedPipeAction.CURRENT_CHART, _patientInfo));
          break;
        default:
          break;
      }
    }

    private void OnNamedPipeConnectChanged(bool isConnected)
    {
      WeakReferenceMessenger.Default.Send(new PipeConnectMessage(new PipeConnectData(isConnected)));
    }

    private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
      try
      {
        // ��׶��� �����忡�� ���ſ� �۾� ���� (UI Automation)
        var patientInfo = await _patientSearchService.FindPatientInfo();
        var previousPatientInfo = _patientInfo;

        // UI �����忡���� ���� ������Ʈ �� �޽��� �߼�
        _uiDispatcher.Invoke(() =>
        {
          _patientInfo = patientInfo;

          if (!patientInfo.IsEqual(previousPatientInfo) && !patientInfo.HasOnlyOneInfo())
          {
            // ��Ʈ��ũ �۾��� �񵿱�� ó�� (UI ������ ����ŷ �� ��)
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
        if (_namedPipeService.IsConnected)
        {
          await _namedPipeService.SendAsync(new NamedPipeData(NamedPipeAction.LOAD_PATIENT, patientInfo));
        }

        WeakReferenceMessenger.Default.Send(new PatientInfoUpdatedMessage(patientInfo));
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error sending patient info: {ex.Message}");
      }
    }

    public void StartReadChartTimer()
    {
      int intervalSec = 1; // (_settingsService.Settings.TargetAppName == AppKey.CustomUserImage) ? 3 : 1;
      _timer.Interval = intervalSec * 1000; // �и��� ����
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
