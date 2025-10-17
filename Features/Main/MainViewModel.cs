using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Messages;
using SpeechAgent.Models;
using SpeechAgent.Services;
using SpeechAgent.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Vanara.PInvoke;

namespace SpeechAgent.Features.Main
{
  partial class MainViewModel : BaseViewModel
  {
    private readonly IViewService _viewService;
    private readonly IMainService _mainService;

    [ObservableProperty]
    private PatientInfo _patInfo = new("", "");


    public MainViewModel(IViewService viewService, IMainService mainService)
    {
      this._viewService = viewService;
      this._mainService = mainService;
    }

    public override void Initialize()
    {
      WeakReferenceMessenger.Default.Register<PatientInfoUpdatedMessage>(this, (_r, m) =>
      {
        PatInfo = m.Value;
      });
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
