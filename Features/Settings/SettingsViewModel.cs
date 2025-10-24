using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpeechAgent.Bases;
using SpeechAgent.Services;

namespace SpeechAgent.Features.Settings
{
  public class Option
  {
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
  }

  partial class SettingsViewModel : BaseViewModel
  {
    public SettingsViewModel(
      ISettingsService settingsService,
      IAutoStartService autoStartService,
      IViewService viewService)
    {
      this._settingsService = settingsService;
      this._autoStartService = autoStartService;
      this._viewService = viewService;
    }

    private readonly ISettingsService _settingsService;
    private readonly IAutoStartService _autoStartService;
    private readonly IViewService _viewService;
    [ObservableProperty]
    private List<Option> options = [];

    [ObservableProperty]
    private string targetAppName = "";

    [ObservableProperty]
    private string connectKey = "";

    [ObservableProperty]
    private bool autoStartEnabled = false;

    [RelayCommand]
    void SaveSettings()
    {
      // Save settings
      _settingsService.UpdateSettings(connectKey: ConnectKey, targetAppName: TargetAppName);
      // Apply auto start setting
      _autoStartService.SetAutoStart(AutoStartEnabled);

      View.Close();
    }

    [RelayCommand]
    void Test()
    {
      _viewService.ShowFindWinView(View);
    }

    [RelayCommand]
    void Close()
    {
      View.Close();
    }

    public override void Initialize()
    {
      Options = [
        new() { Key = "사용안함", Value = "" },
        new() { Key = "클릭", Value = "클릭" },
      ];

      _settingsService.LoadSettings();
      ConnectKey = _settingsService.Settings.ConnectKey;
      TargetAppName = _settingsService.Settings.TargetAppName;

      // Load auto start status from registry
      AutoStartEnabled = _autoStartService.IsAutoStartEnabled();
    }
  }
}
