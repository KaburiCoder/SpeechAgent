using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpeechAgent.Bases;

namespace SpeechAgent.Features.Settings
{
  public class Option
  {
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
  }

  partial class SettingsViewModel : BaseViewModel
  {
    public SettingsViewModel(ISettingsService settingsService)
    {
      PropertyChanged += OnPropertyChanged;
      this._settingsService = settingsService;
    }

    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private List<Option> options = [];

    [ObservableProperty]
    private string appName = "";

    [ObservableProperty]
    private string connectKey = "";

    [RelayCommand]
    void SaveSettings()
    {
      // Save settings
      _settingsService.UpdateSettings(
        connectKey: ConnectKey,
        appName: AppName);
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

      // Load settings
      ConnectKey = _settingsService.ConnectKey;
      AppName = _settingsService.AppName;
    }
  }
}
