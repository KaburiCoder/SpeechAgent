using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Features.Settings;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace SpeechAgent.Features.Main
{
  partial class MainViewModel : BaseViewModel
  {
    private readonly IViewService _viewService;
    private readonly IMainService _mainService;
    private readonly ISettingsService _settingsService;
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
      ISettingsService settingsService)
    {
      this._viewService = viewService;
      this._mainService = mainService;
      this._settingsService = settingsService;

      settingsService.OnConnectKeyChanged += OnConnectKeyChanged;

      // Ping 상태 체크용 타이머 (1초마다)
      _pingTimer = new DispatcherTimer
      {
        Interval = TimeSpan.FromSeconds(1)
      };
      _pingTimer.Tick += OnPingTimerTick;
      _pingTimer.Start();
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

    private async void OnConnectKeyChanged(string connectKey)
    {
      if (string.IsNullOrWhiteSpace(connectKey))
        await _mainService.StopReadChartTimer();
      else
        await _mainService.StartReadChartTimer();
    }

    public override void Initialize()
    {
      WeakReferenceMessenger.Default.Register<PatientInfoUpdatedMessage>(this, (_r, m) =>
      {
        if (PatInfos.Count > 30)
        {
          PatInfos.RemoveAt(PatInfos.Count - 1);
        }

        PatInfos.Insert(0, new PatientInfo(m.Value.Chart, m.Value.Name, DateTime.Now));

      });
      WeakReferenceMessenger.Default.Register<MedicSIOConnectionChangedMessage>(this, (_r, m) =>
      {
        IsSIOConnected = m.Value;
      });
      WeakReferenceMessenger.Default.Register<MedicSIOJoinRoomChangedMessage>(this, (_r, m) =>
      {
        IsJoinedRoom = m.Value;
      });
      WeakReferenceMessenger.Default.Register<WebPingReceivedMessage>(this, (_r, m) =>
      {
        OnWebPingReceived(m.Value);
      });
      OnConnectKeyChanged(_settingsService.ConnectKey);
    }

    [RelayCommand]
    void ShowSettings()
    {
      _viewService.ShowSettingsView(View);
    }

    [RelayCommand]
    void Test()
    {
      _mainService.StopReadChartTimer();
      _mainService.StartReadChartTimer();
    }
  }
}
