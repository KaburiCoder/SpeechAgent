using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Messages;
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
    private Option selectedOption;

    [ObservableProperty]
    private string targetAppName = "";

    [ObservableProperty]
    private string connectKey = "";

    [ObservableProperty]
    private bool autoStartEnabled = false;

    [ObservableProperty]
    private string exeTitle = "";

    [ObservableProperty]
    private string chartClass = "";

[ObservableProperty]
    private string chartIndex = "";

    [ObservableProperty]
    private string nameClass = "";

 [ObservableProperty]
    private string nameIndex = "";

    [ObservableProperty]
    private string customImageName = "";

    [ObservableProperty]
    private string customImageRect = "";

    [ObservableProperty]
    private bool useAutomation = false;

    public bool IsCustomSelected => SelectedOption?.Value == "[사용자 정의]";
    public bool IsCustomImageSelected => SelectedOption?.Value == "[사용자 정의(이미지)]";

    partial void OnSelectedOptionChanged(Option value)
    {
      OnPropertyChanged(nameof(IsCustomSelected));
      OnPropertyChanged(nameof(IsCustomImageSelected));
  }

    [RelayCommand]
    void SaveSettings()
    {
      TargetAppName = SelectedOption?.Value ?? "";
      // Save settings
      _settingsService.UpdateSettings(
        connectKey: ConnectKey, 
        targetAppName: TargetAppName, 
        customExeTitle: ExeTitle, 
        customChartClass: ChartClass, 
 customChartIndex: ChartIndex, 
        customNameClass: NameClass, 
   customNameIndex: NameIndex,
    customImageName: CustomImageName,
        customImageRect: CustomImageRect,
        useAutomation: UseAutomation);
      // Apply auto start setting
      _autoStartService.SetAutoStart(AutoStartEnabled);

      View.Close();
    }

    [RelayCommand]
    void FindControl()
    {
      _viewService.ShowFindWinView(View);
    }

    [RelayCommand]
    void FindImageControl()
    {
      _viewService.ShowFindWinImageView(View);
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
        new() { Key = "[사용자 정의]", Value = "[사용자 정의]" },
  new() { Key = "[사용자 정의(이미지)]", Value = "[사용자 정의(이미지)]" },
      ];

      _settingsService.LoadSettings();
      ConnectKey = _settingsService.Settings.ConnectKey;
      TargetAppName = _settingsService.Settings.TargetAppName;
      ExeTitle = _settingsService.Settings.CustomExeTitle;
      ChartClass = _settingsService.Settings.CustomChartClass;
      ChartIndex = _settingsService.Settings.CustomChartIndex;
      NameClass = _settingsService.Settings.CustomNameClass;
      NameIndex = _settingsService.Settings.CustomNameIndex;
      CustomImageName = _settingsService.Settings.CustomImageName;
    CustomImageRect = _settingsService.Settings.CustomImageRect;
      UseAutomation = _settingsService.Settings.UseAutomation;

      SelectedOption = Options.FirstOrDefault(o => o.Value == TargetAppName) ?? Options[0];

      // Load auto start status from registry
      AutoStartEnabled = _autoStartService.IsAutoStartEnabled();

      WeakReferenceMessenger.Default.Register<SendToSettingsMessage>(this, (r, m) =>
      {
        ExeTitle = m.Value.ExeTitle;
        ChartClass = m.Value.ChartClass;
 ChartIndex = m.Value.ChartIndex;
    NameClass = m.Value.NameClass;
        NameIndex = m.Value.NameIndex;
      });

      WeakReferenceMessenger.Default.Register<SendToSettingsImageMessage>(this, (r, m) =>
      {
        CustomImageName = m.Value.CustomImageName;
      CustomImageRect = m.Value.CustomImageRect;
   });
    }
  }
}
