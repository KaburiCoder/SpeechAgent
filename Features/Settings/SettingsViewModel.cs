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
    public SettingsViewModel(ISettingsService settingsService, IAutoStartService autoStartService)
    {
      PropertyChanged += OnPropertyChanged;
      this._settingsService = settingsService;
      this._autoStartService = autoStartService;
    }

    private readonly ISettingsService _settingsService;
    private readonly IAutoStartService _autoStartService;

    [ObservableProperty]
    private List<Option> options = [];

    [ObservableProperty]
    private string appName = "";

    [ObservableProperty]
    private string connectKey = "";

    [ObservableProperty]
    private bool autoStartEnabled = false;

    [RelayCommand]
    void SaveSettings()
    {
      // Save settings
      _settingsService.UpdateSettings(
        connectKey: ConnectKey,
      appName: AppName);

  // Apply auto start setting
      _autoStartService.SetAutoStart(AutoStartEnabled);
    }

    [RelayCommand]
    void Close()
    {
  View.Close();
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
  {
      if (e.PropertyName == nameof(AppName))
  {
        OnSelectedValueChanged();
      }
    }

private void OnSelectedValueChanged()
    {
      // Handle value change here
Console.WriteLine($"Selected value changed to: {AppName}");
    }

    public override void Initialize()
    {
      Options = [
   new() { Key = "클릭", Value = "클릭" },
 ];

    _settingsService.LoadSettings(); 
      ConnectKey = _settingsService.ConnectKey;
      AppName = _settingsService.AppName;
      
      // Load auto start status from registry
      AutoStartEnabled = _autoStartService.IsAutoStartEnabled();
    }
  }
}
