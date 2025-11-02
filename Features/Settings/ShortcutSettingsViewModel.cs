using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpeechAgent.Bases;
using SpeechAgent.Database.Schemas;

namespace SpeechAgent.Features.Settings
{
  public partial class ShortcutSettingsViewModel : BaseViewModel
  {
    private readonly IShortcutSettingsService _shortcutSettingsService;

    #region Observable Properties

    /// <summary>
    /// CC - 수정자 키
    /// </summary>
    [ObservableProperty]
    private ModifierKeys ccModifiers = ModifierKeys.None;

    /// <summary>
    /// CC - 키
    /// </summary>
    [ObservableProperty]
    private Key ccKey = Key.None;

    /// <summary>
    /// SOAP 전체 - 수정자 키
    /// </summary>
    [ObservableProperty]
    private ModifierKeys allModifiers = ModifierKeys.None;

    /// <summary>
    /// SOAP 전체 - 키
    /// </summary>
    [ObservableProperty]
    private Key allKey = Key.None;

    /// <summary>
    /// S (Subjective) - 수정자 키
    /// </summary>
    [ObservableProperty]
    private ModifierKeys sModifiers = ModifierKeys.None;

    /// <summary>
    /// S (Subjective) - 키
    /// </summary>
    [ObservableProperty]
    private Key sKey = Key.None;

    /// <summary>
    /// O (Objective) - 수정자 키
    /// </summary>
    [ObservableProperty]
    private ModifierKeys oModifiers = ModifierKeys.None;

    /// <summary>
    /// O (Objective) - 키
    /// </summary>
    [ObservableProperty]
    private Key oKey = Key.None;

    /// <summary>
    /// A (Assessment) - 수정자 키
    /// </summary>
    [ObservableProperty]
    private ModifierKeys aModifiers = ModifierKeys.None;

    /// <summary>
    /// A (Assessment) - 키
    /// </summary>
    [ObservableProperty]
    private Key aKey = Key.None;

    /// <summary>
    /// P (Plan) - 수정자 키
    /// </summary>
    [ObservableProperty]
    private ModifierKeys pModifiers = ModifierKeys.None;

    /// <summary>
    /// P (Plan) - 키
    /// </summary>
    [ObservableProperty]
    private Key pKey = Key.None;

    #endregion

    public ShortcutSettingsViewModel(IShortcutSettingsService shortcutSettingsService)
    {
      _shortcutSettingsService = shortcutSettingsService;
    }

    #region Commands

    [RelayCommand]
    private void Close()
    {
      View.Close();
    }

    [RelayCommand]
    private void Save()
    {
      try
      {
        _shortcutSettingsService.SaveShortcut(AllModifiers, AllKey, ShortcutFeature.All);
        _shortcutSettingsService.SaveShortcut(CcModifiers, CcKey, ShortcutFeature.CC);
        _shortcutSettingsService.SaveShortcut(SModifiers, SKey, ShortcutFeature.S);
        _shortcutSettingsService.SaveShortcut(OModifiers, OKey, ShortcutFeature.O);
        _shortcutSettingsService.SaveShortcut(AModifiers, AKey, ShortcutFeature.A);
        _shortcutSettingsService.SaveShortcut(PModifiers, PKey, ShortcutFeature.P);

        View.Close();
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error saving shortcuts: {ex.Message}");
      }
    }

    #endregion

    #region Methods

    public override void Initialize()
    {
      LoadShortcuts();
    }

    /// <summary>
    /// 저장된 단축키 로드
    /// </summary>
    private void LoadShortcuts()
    {
      try
      {
        // CC 로드
        var ccShortcut = _shortcutSettingsService.GetShortcut(ShortcutFeature.CC);
        if (ccShortcut != null)
        {
          CcModifiers = ccShortcut.Modifiers;
          CcKey = ccShortcut.Key;
        }

        // SOAP 전체 로드
        var allShortcut = _shortcutSettingsService.GetShortcut(ShortcutFeature.All);
        if (allShortcut != null)
        {
          AllModifiers = allShortcut.Modifiers;
          AllKey = allShortcut.Key;
        }

        // S 로드
        var sShortcut = _shortcutSettingsService.GetShortcut(ShortcutFeature.S);
        if (sShortcut != null)
        {
          SModifiers = sShortcut.Modifiers;
          SKey = sShortcut.Key;
        }

        // O 로드
        var oShortcut = _shortcutSettingsService.GetShortcut(ShortcutFeature.O);
        if (oShortcut != null)
        {
          OModifiers = oShortcut.Modifiers;
          OKey = oShortcut.Key;
        }

        // A 로드
        var aShortcut = _shortcutSettingsService.GetShortcut(ShortcutFeature.A);
        if (aShortcut != null)
        {
          AModifiers = aShortcut.Modifiers;
          AKey = aShortcut.Key;
        }

        // P 로드
        var pShortcut = _shortcutSettingsService.GetShortcut(ShortcutFeature.P);
        if (pShortcut != null)
        {
          PModifiers = pShortcut.Modifiers;
          PKey = pShortcut.Key;
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine($"Error loading shortcuts: {ex.Message}");
      }
    }

    #endregion
  }
}
