using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Features.Settings;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;

namespace SpeechAgent.Features.Main
{
  partial class MainViewModel : BaseViewModel
  {
    private readonly IViewService _viewService;
    private readonly IMainService _mainService;
    private readonly ISettingsService _settingsService;
    [ObservableProperty]
    private PatientInfo _patInfo = new("", "");


    public MainViewModel(
      IViewService viewService,
      IMainService mainService,
      ISettingsService settingsService)
    {
      this._viewService = viewService;
      this._mainService = mainService;
      this._settingsService = settingsService;
      settingsService.OnConnectKeyChanged += OnConnectKeyChanged;
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
        PatInfo = m.Value;
      });

      OnConnectKeyChanged(_settingsService.ConnectKey);
    }

    [RelayCommand]
    void ShowSettings()
    {
      _viewService.ShowSettingsView();
    }

    [RelayCommand]
    void Test()
    {
      _mainService.StopReadChartTimer();
      _mainService.StartReadChartTimer();
    }
  }
}
