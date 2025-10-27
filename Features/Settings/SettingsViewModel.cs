using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SpeechAgent.Bases;
using SpeechAgent.Constants;
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
    private Option? selectedOption = null;

    [ObservableProperty]
    private string targetAppName = "";

    [ObservableProperty]
    private string connectKey = "";

    [ObservableProperty]
    private bool autoStartEnabled = false;

    [ObservableProperty]
    private string exeTitle = "";

    [ObservableProperty]
    private string chartControlType = "";

    [ObservableProperty]
    private string chartIndex = "";

    [ObservableProperty]
    private string nameControlType = "";

    [ObservableProperty]
    private string nameIndex = "";

    [ObservableProperty]
    private string customImageRect = "";

    public bool IsCustomSelected => SelectedOption?.Value == AppKey.CustomUser;
    public bool IsCustomImageSelected => SelectedOption?.Value == AppKey.CustomUserImage;
    public bool IsCustomWinApiSelected => SelectedOption?.Value == AppKey.CustomUserWinApi;

    partial void OnSelectedOptionChanged(Option? value)
    {
      OnPropertyChanged(nameof(IsCustomSelected));
      OnPropertyChanged(nameof(IsCustomImageSelected));
      OnPropertyChanged(nameof(IsCustomWinApiSelected));
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
        customChartControlType: ChartControlType,
        customChartIndex: ChartIndex,
        customNameControlType: NameControlType,
        customNameIndex: NameIndex,
        customImageRect: CustomImageRect);
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
    void FindWinApiControl()
    {
      _viewService.ShowFindWinApiView(View);
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
        new() { Key = "없음", Value = AppKey.None },
        new() { Key = "A 클릭", Value = AppKey.ClickSoft },
        new() { Key = "B 의사랑", Value = AppKey.USarang },
        new() { Key = AppKey.CustomUser, Value =AppKey.CustomUser },
        //new() { Key = "사용자 정의 WinAPI", Value = AppKey.CustomUserWinApi },
        new() { Key = AppKey.CustomUserImage, Value = AppKey.CustomUserImage },
      ];

      _settingsService.LoadSettings();
      ConnectKey = _settingsService.Settings.ConnectKey;
      TargetAppName = _settingsService.Settings.TargetAppName;
      ExeTitle = _settingsService.Settings.CustomExeTitle;
      ChartControlType = _settingsService.Settings.CustomChartControlType;
      ChartIndex = _settingsService.Settings.CustomChartIndex;
      NameControlType = _settingsService.Settings.CustomNameControlType;
      NameIndex = _settingsService.Settings.CustomNameIndex; 
      CustomImageRect = _settingsService.Settings.CustomImageRect;

      SelectedOption = Options.FirstOrDefault(o => o.Value == TargetAppName) ?? Options[0];

      // Load auto start status from registry
      AutoStartEnabled = _autoStartService.IsAutoStartEnabled();

      WeakReferenceMessenger.Default.Register<SendToSettingsMessage>(this, (r, m) =>
      {
        ExeTitle = m.Value.ExeTitle;
        ChartControlType = m.Value.ChartControlType;
        ChartIndex = m.Value.ChartIndex;
        NameControlType = m.Value.NameControlType;
        NameIndex = m.Value.NameIndex;
      });

      WeakReferenceMessenger.Default.Register<SendToSettingsImageMessage>(this, (r, m) =>
      {
        ExeTitle = m.Value.CustomExeTitle;
        CustomImageRect = m.Value.CustomImageRect;
      });
    }
  }
}
