using SpeechAgent.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace SpeechAgent.Features.Settings
{
  public class Option
  {
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
  }

  partial class SettingsViewModel : BaseViewModel
  {
    [ObservableProperty]
    private List<Option> options = [];

    [ObservableProperty]
    private string? appName;

    [ObservableProperty]
    private string? connectKey;

    [RelayCommand]
    void SaveSettings()
    {
      // Save settings
      setting.Default.CONNECT_KEY = ConnectKey ?? string.Empty;
      setting.Default.APP_NAME = AppName ?? string.Empty;
      setting.Default.Save();
    }

    [RelayCommand]
    void Close()
    {
      View.Close();
    }

    public SettingsViewModel()
    {
      PropertyChanged += OnPropertyChanged;
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
        new() { Key = "eClick(신진료실)", Value = "e_new" },
        new() { Key = "eClick(구진료실)", Value = "e_old" }
      ];

      // Load settings
      ConnectKey = setting.Default.CONNECT_KEY;
      AppName = setting.Default.APP_NAME;
    }
  }
}
