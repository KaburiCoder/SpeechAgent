using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Features.Settings;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using SpeechAgent.Services.MedicSIO;
using System.Collections.ObjectModel;

namespace SpeechAgent.Features.Main
{
  partial class MainViewModel : BaseViewModel
  {
    private readonly IViewService _viewService;
    private readonly IMainService _mainService;
    private readonly ISettingsService _settingsService;
    private readonly IUpdateService _updateService;
    
    [ObservableProperty]
    private ObservableCollection<PatientInfo> _patInfos = new();
    
    [ObservableProperty]
    private bool _isSIOConnected = false;
    [ObservableProperty]
    private bool _isJoinedRoom = false;

    [ObservableProperty]
    private bool _isUpdateAvailable = false;
    [ObservableProperty]
    private string _updateVersion = string.Empty;

    public MainViewModel(
      IViewService viewService,
      IMainService mainService,
      ISettingsService settingsService,
      IUpdateService updateService)
    {
      this._viewService = viewService;
      this._mainService = mainService;
      this._settingsService = settingsService;
      this._updateService = updateService;
      
      settingsService.OnConnectKeyChanged += OnConnectKeyChanged;
      
      // 업데이트 이벤트 구독
      _updateService.UpdateAvailable += OnUpdateAvailable;
    }

    private void OnUpdateAvailable(object? sender, UpdateAvailableEventArgs e)
    {
      IsUpdateAvailable = true;
      UpdateVersion = e.NewVersion;
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
      OnConnectKeyChanged(_settingsService.ConnectKey);
    }

    [RelayCommand]
    void ShowSettings()
    {
      _viewService.ShowSettingsView(View);
    }

    [RelayCommand]
    async Task CheckForUpdates()
    {
      await _updateService.CheckForUpdatesAsync();
    }

    [RelayCommand]
    void Test()
    {
      _mainService.StopReadChartTimer();
      _mainService.StartReadChartTimer();
    }
  }
}
