using System.Diagnostics;
using System.Windows.Input;
using Gma.System.MouseKeyHook;
using SpeechAgent.Database.Schemas;
using SpeechAgent.Features.Settings;
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

    public GlobalKeyHook(IShortcutSettingsService shortcutSettingsService)
    {
      this._shortcutSettingsService = shortcutSettingsService;
    }

    public void Start()
    {
      _shortcuts = _shortcutSettingsService.LoadAllShortcuts();

      // 글로벌 훅 등록
      _hook = Hook.GlobalEvents();
      _hook.KeyDown += _hook_KeyDown;
    }

    private void _hook_KeyDown(object? sender, System.Windows.Forms.KeyEventArgs e)
    {
      var wpfKey = KeyInterop.KeyFromVirtualKey((int)e.KeyCode);
      var foundShortcut = _shortcuts.Find(s =>
        s.Modifiers == Keyboard.Modifiers && s.Key == wpfKey
      );

      if (foundShortcut == null)
        return;

      switch (foundShortcut!.ShortcutFeature)
      {
        case ShortcutFeature.All:
          Debug.WriteLine("Global shortcut triggered: All features");
          e.Handled = true;
          break;
        case ShortcutFeature.CC:
          Debug.WriteLine("Global shortcut triggered: CC feature");
          e.Handled = true;
          break;
        case ShortcutFeature.S:
          Debug.WriteLine("Global shortcut triggered: S feature");
          e.Handled = true;
          break;
        case ShortcutFeature.O:
          Debug.WriteLine("Global shortcut triggered: O feature");
          e.Handled = true;
          break;
        case ShortcutFeature.A:
          Debug.WriteLine("Global shortcut triggered: A feature");
          e.Handled = true;
          break;
        case ShortcutFeature.P:
          Debug.WriteLine("Global shortcut triggered: P feature");
          e.Handled = true;
          break;
      }
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
