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
      IViewService viewService
    )
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
    private string exeTitle = "";

    [ObservableProperty]
    private string chartControlType = "";

    [ObservableProperty]
    private string chartIndex = "";

    [ObservableProperty]
    private string chartRegex = "";

    [ObservableProperty]
    private string chartRegexIndex = "0";

    [ObservableProperty]
    private string nameRegex = "";

    [ObservableProperty]
    private string nameRegexIndex = "0";

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
        targetAppName: TargetAppName,
        customExeTitle: ExeTitle,
        customChartControlType: ChartControlType,
        customChartIndex: ChartIndex,
        customChartRegex: ChartRegex,
        customChartRegexIndex: ChartRegexIndex,
        customNameControlType: NameControlType,
        customNameIndex: NameIndex,
        customNameRegex: NameRegex,
        customNameRegexIndex: NameRegexIndex,
        customImageRect: CustomImageRect
      );
      // 기존 레지스트리는 무조건 제거
      _autoStartService.SetAutoStartLegacy(false);

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
      Options =
      [
        new() { Key = "없음", Value = AppKey.None },
        new() { Key = "A사", Value = AppKey.ClickSoft },
        new() { Key = "B사", Value = AppKey.USarang },
        new() { Key = "C사", Value = AppKey.Brain },
        new() { Key = "D사", Value = AppKey.Doctors },
        new() { Key = "E사", Value = AppKey.DRChart },
        new() { Key = "F사", Value = AppKey.BitUChart },
        new() { Key = AppKey.CustomUser, Value = AppKey.CustomUser },
        //new() { Key = "사용자 정의 WinAPI", Value = AppKey.CustomUserWinApi },
        new() { Key = AppKey.CustomUserImage, Value = AppKey.CustomUserImage },
      ];

      _settingsService.LoadSettings();
      TargetAppName = _settingsService.Settings.TargetAppName;
      ExeTitle = _settingsService.Settings.CustomExeTitle;
      ChartControlType = _settingsService.Settings.CustomChartControlType;
      ChartIndex = _settingsService.Settings.CustomChartIndex;
      ChartRegex = _settingsService.Settings.CustomChartRegex;
      ChartRegexIndex = _settingsService.Settings.CustomChartRegexIndex.ToString();
      NameControlType = _settingsService.Settings.CustomNameControlType;
      NameIndex = _settingsService.Settings.CustomNameIndex;
      NameRegex = _settingsService.Settings.CustomNameRegex;
      NameRegexIndex = _settingsService.Settings.CustomNameRegexIndex.ToString();
      CustomImageRect = _settingsService.Settings.CustomImageRect;

      SelectedOption = Options.FirstOrDefault(o => o.Value == TargetAppName) ?? Options[0];

      WeakReferenceMessenger.Default.Register<SendToSettingsMessage>(
        this,
        (r, m) =>
        {
          ExeTitle = m.Value.ExeTitle;
          ChartControlType = m.Value.ChartControlType;
          ChartIndex = m.Value.ChartIndex;
          ChartRegex = m.Value.ChartRegex;
          ChartRegexIndex = m.Value.ChartRegexIndex;
          NameControlType = m.Value.NameControlType;
          NameIndex = m.Value.NameIndex;
          NameRegex = m.Value.NameRegex;
          NameRegexIndex = m.Value.NameRegexIndex;
        }
      );

      WeakReferenceMessenger.Default.Register<SendToSettingsImageMessage>(
        this,
        (r, m) =>
        {
          ExeTitle = m.Value.CustomExeTitle;
          CustomImageRect = m.Value.CustomImageRect;
        }
      );
    }
  }
}
