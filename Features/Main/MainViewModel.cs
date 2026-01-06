using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Database.Schemas;
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

    [ObservableProperty]
    private ObservableCollection<PatientInfo> _patInfos = new();

    [ObservableProperty]
    private bool _isPipeConnected = false;

    public MainViewModel(
      IViewService viewService,
      IMainService mainService,
      ISettingsService settingsService
    )
    {
      this._viewService = viewService;
      this._mainService = mainService;
      this._settingsService = settingsService;
    }

    private async void OnLocalSettingsChanged(LocalSettings settings)
    {
      if (string.IsNullOrWhiteSpace(settings.TargetAppName))
        _mainService.StopReadChartTimer();
      else
        _mainService.StartReadChartTimer();
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

      WeakReferenceMessenger.Default.Register<LocalSettingsChangedMessage>(
        this,
        (_r, m) =>
        {
          OnLocalSettingsChanged(m.Value.Settings);
        }
      );

      WeakReferenceMessenger.Default.Register<PipeConnectMessage>(
        this,
        (_r, m) =>
        {
          Dispatcher.CurrentDispatcher.Invoke(() =>
          {
            IsPipeConnected = m.Value.IsConnected;
          });
        }
      );
      OnLocalSettingsChanged(_settingsService.Settings);
    }

    [RelayCommand]
    void ShowSettings()
    {
      _viewService.ShowSettingsView(View);
    }
  }
}
