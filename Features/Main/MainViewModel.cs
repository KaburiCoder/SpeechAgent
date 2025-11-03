using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Database.Schemas;
using SpeechAgent.Features.Settings;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using SpeechAgent.Services.Globals;
using SpeechAgent.Services.MedicSIO;
using SpeechAgent.Utils;

namespace SpeechAgent.Features.Main
{
  partial class MainViewModel : BaseViewModel
  {
    private readonly IViewService _viewService;
    private readonly IMainService _mainService;
    private readonly ISettingsService _settingsService;
    private readonly IMedicSIOService _medicSIOService;
    private readonly IGlobalKeyHook _globalKeyHook;
    private readonly DispatcherTimer _pingTimer;

    [ObservableProperty]
    private ObservableCollection<PatientInfo> _patInfos = new();

    [ObservableProperty]
    private bool _isSIOConnected = false;

    [ObservableProperty]
    private bool _isJoinedRoom = false;

    [ObservableProperty]
    private DateTime? _lastPingTime = null;

    [ObservableProperty]
    private bool _isPingHealthy = false;

    [ObservableProperty]
    private string _pingStatusText = "연결 대기 중";

    public MainViewModel(
      IViewService viewService,
      IMainService mainService,
      ISettingsService settingsService,
      IMedicSIOService medicSIOService,
      IGlobalKeyHook globalKeyHook,
      IUpdateService updateService
    )
    {
      this._viewService = viewService;
      this._mainService = mainService;
      this._settingsService = settingsService;
      this._medicSIOService = medicSIOService;
      this._globalKeyHook = globalKeyHook;

      // 키보드 후킹
      _globalKeyHook.Start();

      // Ping 상태 체크용 타이머 (1초마다)
      _pingTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
      _pingTimer.Tick += OnPingTimerTick;
      _pingTimer.Start();

      BootPopupBrowser(updateService);
    }

    private void BootPopupBrowser(IUpdateService updateService)
    {
      var isUpdateApplied = updateService.IsUpdateApplied;
      updateService.IsUpdateApplied = false; // 플래그 초기화
      if (isUpdateApplied)
        return; // 업데이트 후 부팅: 아무 동작도 하지 않음

      // 일반 부팅: IsBootPopupBrowserEnabled가 true이면 VoiceMedic 열기
      if (_settingsService.Settings.IsBootPopupBrowserEnabled)
      {
        BrowserLauncher.OpenMedic();
      }
    }

    private void OnWebPingReceived(DateTime pingTime)
    {
      LastPingTime = pingTime;
      UpdatePingStatus();
    }

    private void OnPingTimerTick(object? sender, EventArgs e)
    {
      UpdatePingStatus();
    }

    private void UpdatePingStatus()
    {
      if (LastPingTime == null)
      {
        IsPingHealthy = false;
        PingStatusText = "연결 대기 중";
        return;
      }

      var elapsed = DateTime.Now - LastPingTime.Value;
      var secondsElapsed = (int)elapsed.TotalSeconds;

      if (secondsElapsed <= 10)
      {
        IsPingHealthy = true;
        PingStatusText = $"{secondsElapsed}초 전";
      }
      else
      {
        IsPingHealthy = false;
        if (secondsElapsed < 60)
        {
          PingStatusText = $"{secondsElapsed}초 전";
        }
        else
        {
          var minutesElapsed = (int)elapsed.TotalMinutes;
          PingStatusText = $"{minutesElapsed}분 전";
        }
      }
    }

    private async void OnConnectKeyChanged(LocalSettings settings)
    {
      if (string.IsNullOrWhiteSpace(settings.ConnectKey))
      {
        _mainService.StopReadChartTimer();
        await _medicSIOService.DisConnect();
        return;
      }

      if (string.IsNullOrWhiteSpace(settings.TargetAppName))
        _mainService.StopReadChartTimer();
      else
        _mainService.StartReadChartTimer();

      if (string.IsNullOrWhiteSpace(settings.ConnectKey))
        await _medicSIOService.DisConnect();
      else
        await _medicSIOService.Connect();
    }

    public override void Initialize()
    {
      WeakReferenceMessenger.Default.Register<PatientInfoUpdatedMessage>(
        this,
        (_r, m) =>
        {
          if (PatInfos.Count > 30)
          {
            PatInfos.RemoveAt(PatInfos.Count - 1);
          }

          PatInfos.Insert(0, new PatientInfo(m.Value.Chart, m.Value.Name, DateTime.Now));
        }
      );
      WeakReferenceMessenger.Default.Register<MedicSIOConnectionChangedMessage>(
        this,
        (_r, m) =>
        {
          IsSIOConnected = m.Value;
        }
      );
      WeakReferenceMessenger.Default.Register<MedicSIOJoinRoomChangedMessage>(
        this,
        (_r, m) =>
        {
          IsJoinedRoom = m.Value;
        }
      );
      WeakReferenceMessenger.Default.Register<WebPingReceivedMessage>(
        this,
        (_r, m) =>
        {
          OnWebPingReceived(m.Value);
        }
      );
      WeakReferenceMessenger.Default.Register<LocalSettingsChangedMessage>(
        this,
        (_r, m) =>
        {
          OnConnectKeyChanged(m.Value.Settings);
        }
      );
      OnConnectKeyChanged(_settingsService.Settings);
    }

    [RelayCommand]
    void ShowSettings()
    {
      _viewService.ShowSettingsView(View);
    }

    [RelayCommand]
    void ShowShortcutSettings()
    {
      _globalKeyHook.Stop();
      _viewService.ShowShortcutSettingsView(View);
      _globalKeyHook.Start();
    }
  }
}
