using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpeechAgent.Bases;
using SpeechAgent.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechAgent.Features.Main
{
  partial class MainViewModel : BaseViewModel
  {
    private readonly IViewService _viewService;

    public MainViewModel(IViewService viewService)
    {
      this._viewService = viewService;
    }

    public override void Initialize()
    {

    }

    [RelayCommand]
    void ShowSettings()
    {
      _viewService.ShowSettingsView();
    }
  }
}
