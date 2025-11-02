using System.Diagnostics;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SpeechAgent.Database.Schemas;
using SpeechAgent.Features.Settings;
using SpeechAgent.Services.MedicSIO;
using SpeechAgent.Services.MedicSIO.Dto;
using SpeechAgent.Utils;
using static Vanara.PInvoke.User32;

namespace SpeechAgent.Services.Globals
{
  public interface IGlobalKeyHook
  {
    void Start();
    void Stop();
  }

  internal class GlobalKeyHook : IGlobalKeyHook
  {
    private IKeyboardMouseEvents? _hook;
    private List<CustomShortcuts> _shortcuts = [];
    private readonly IShortcutSettingsService _shortcutSettingsService;
    private readonly IMedicSIOService _medicSIOService;

    public GlobalKeyHook(
      IShortcutSettingsService shortcutSettingsService,
      IMedicSIOService medicSIOService
    )
    {
      this._shortcutSettingsService = shortcutSettingsService;
      this._medicSIOService = medicSIOService;
    }

    public void Start()
    {
      _shortcuts = _shortcutSettingsService.LoadAllShortcuts();

      // 글로벌 훅 등록
      _hook = Hook.GlobalEvents();
      _hook.KeyDown += _hook_KeyDown;
    }

    private async void _hook_KeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
    {
      var wpfKey = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
      var foundShortcut = _shortcuts.Find(s =>
        s.Modifiers == Keyboard.Modifiers && s.Key == wpfKey
      );

      if (foundShortcut == null)
        return;

      switch (foundShortcut.ShortcutFeature)
      {
        case ShortcutFeature.All:
        case ShortcutFeature.CC:
        case ShortcutFeature.S:
        case ShortcutFeature.O:
        case ShortcutFeature.A:
        case ShortcutFeature.P:
          e.Handled = true;
          break;
        default:
          return;
      }

      var res = await _medicSIOService.RequestSummary(
        new RequestSummaryDto { Key = $"{foundShortcut.ShortcutFeature}" }
      );
      Debug.Print(res.ToString());
    }

    public void Stop()
    {
      if (_hook == null)
        return;

      _hook.KeyDown -= _hook_KeyDown;
      _hook.Dispose();
    }
  }
}
