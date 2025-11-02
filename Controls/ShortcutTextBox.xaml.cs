using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpeechAgent.Controls
{
  /// <summary>
  /// ShortcutTextBox.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class ShortcutTextBox : System.Windows.Controls.UserControl
  {
    public ShortcutTextBox()
    {
      InitializeComponent();
      ShortcutDisplayTextBox.PreviewKeyDown += ShortcutDisplayTextBox_PreviewKeyDown;
    }

    #region Dependency Properties

    /// <summary>
    /// 입력된 수정자 키 (Ctrl, Alt, Shift, Win)
    /// </summary>
    public ModifierKeys InputModifiers
    {
      get { return (ModifierKeys)GetValue(InputModifiersProperty); }
      set { SetValue(InputModifiersProperty, value); }
    }

    public static readonly DependencyProperty InputModifiersProperty = DependencyProperty.Register(
      nameof(InputModifiers),
      typeof(ModifierKeys),
      typeof(ShortcutTextBox),
      new PropertyMetadata(ModifierKeys.None, OnInputModifiersChanged)
    );

    private static void OnInputModifiersChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e
    )
    {
      var control = (ShortcutTextBox)d;
      control.UpdateTextBoxDisplay();
    }

    /// <summary>
    /// 입력된 키
    /// </summary>
    public Key InputKey
    {
      get { return (Key)GetValue(InputKeyProperty); }
      set { SetValue(InputKeyProperty, value); }
    }

    public static readonly DependencyProperty InputKeyProperty = DependencyProperty.Register(
      nameof(InputKey),
      typeof(Key),
      typeof(ShortcutTextBox),
      new PropertyMetadata(Key.None, OnInputKeyChanged)
    );

    private static void OnInputKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var control = (ShortcutTextBox)d;
      control.UpdateTextBoxDisplay();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// TextBox 표시 업데이트
    /// </summary>
    private void UpdateTextBoxDisplay()
    {
      // InputModifiers와 InputKey 모두 None인 경우
      if (InputModifiers == ModifierKeys.None && InputKey == Key.None)
      {
        ShortcutDisplayTextBox.Text = "";
        return;
      }

      // 유효하지 않은 조합 확인
      bool isValidKey = IsValidKey(InputKey);

      if (!isValidKey)
      {
        ShortcutDisplayTextBox.Text = "";
        return;
      }

      // 텍스트 업데이트
      var hotKeyText = BuildHotKeyText(InputModifiers, InputKey);
      ShortcutDisplayTextBox.Text = hotKeyText;
    }

    /// <summary>
    /// 유효한 키인지 확인
    /// </summary>
    private bool IsValidKey(Key key)
    {
      // 특수키인 경우 유효
      if (IsSpecialKey(key))
        return true;

      // 일반 문자/숫자도 수정자와 함께라면 유효
      if (InputModifiers != ModifierKeys.None && !IsModifierKey(key) && key != Key.None)
        return true;

      return false;
    }

    /// <summary>
    /// 특수 키인지 확인 (F1~F12, Enter, Tab, Esc 등)
    /// </summary>
    private bool IsSpecialKey(Key key)
    {
      return key >= Key.F1 && key <= Key.F12
        || key == Key.Scroll
        || key == Key.PrintScreen
        || key == Key.Pause;
    }

    /// <summary>
    /// 수정자 키인지 확인
    /// </summary>
    private bool IsModifierKey(Key key)
    {
      return key == Key.LeftCtrl
        || key == Key.RightCtrl
        || key == Key.LeftAlt
        || key == Key.RightAlt
        || key == Key.LeftShift
        || key == Key.RightShift
        || key == Key.LWin
        || key == Key.RWin;
    }

    /// <summary>
    /// Key enum을 읽기 쉬운 문자열로 변환
    /// </summary>
    private string GetKeyName(Key key)
    {
      return key switch
      {
        Key.A => "A",
        Key.B => "B",
        Key.C => "C",
        Key.D => "D",
        Key.E => "E",
        Key.F => "F",
        Key.G => "G",
        Key.H => "H",
        Key.I => "I",
        Key.J => "J",
        Key.K => "K",
        Key.L => "L",
        Key.M => "M",
        Key.N => "N",
        Key.O => "O",
        Key.P => "P",
        Key.Q => "Q",
        Key.R => "R",
        Key.S => "S",
        Key.T => "T",
        Key.U => "U",
        Key.V => "V",
        Key.W => "W",
        Key.X => "X",
        Key.Y => "Y",
        Key.Z => "Z",
        Key.D0 => "0",
        Key.D1 => "1",
        Key.D2 => "2",
        Key.D3 => "3",
        Key.D4 => "4",
        Key.D5 => "5",
        Key.D6 => "6",
        Key.D7 => "7",
        Key.D8 => "8",
        Key.D9 => "9",
        Key.F1 => "F1",
        Key.F2 => "F2",
        Key.F3 => "F3",
        Key.F4 => "F4",
        Key.F5 => "F5",
        Key.F6 => "F6",
        Key.F7 => "F7",
        Key.F8 => "F8",
        Key.F9 => "F9",
        Key.F10 => "F10",
        Key.F11 => "F11",
        Key.F12 => "F12",
        Key.OemComma => ",",
        Key.OemPeriod => ".",
        Key.OemMinus => "-",
        Key.OemPlus => "+",
        Key.Space => "Space",
        Key.Return => "Enter",
        Key.Tab => "Tab",
        Key.Escape => "Esc",
        Key.Delete => "Del",
        Key.Back => "Backspace",
        Key.Insert => "Ins",
        Key.Home => "Home",
        Key.End => "End",
        Key.PageUp => "PgUp",
        Key.PageDown => "PgDn",
        Key.Left => "←",
        Key.Right => "→",
        Key.Up => "↑",
        Key.Down => "↓",
        Key.PrintScreen => "PrintScreen",
        Key.Pause => "Pause",
        _ => key.ToString(),
      };
    }

    /// <summary>
    /// 수정자 키와 일반 키를 조합하여 문자열 생성
    /// </summary>
    private string BuildHotKeyText(ModifierKeys modifiers, Key key)
    {
      var parts = new List<string>();

      if ((modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        parts.Add("Ctrl");

      if ((modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
        parts.Add("Alt");

      if ((modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        parts.Add("Shift");

      if ((modifiers & ModifierKeys.Windows) == ModifierKeys.Windows)
        parts.Add("Win");

      if (key != Key.None && !IsModifierKey(key))
      {
        var keyName = GetKeyName(key);
        parts.Add(keyName);
      }

      return string.Join("+", parts);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// TextBox에서 키 입력 감지
    /// </summary>
    private void ShortcutDisplayTextBox_PreviewKeyDown(
      object sender,
      System.Windows.Input.KeyEventArgs e
    )
    {
      var modifiers = Keyboard.Modifiers;
      var key = e.Key;

      // Alt 키가 눌렸을 때는 SystemKey를 확인 (WPF의 특수 동작)
      if (key == Key.System)
      {
        key = e.SystemKey;
      }

      // 수정자 키만 눌린 경우 무시
      if (IsModifierKey(key))
      {
        ShortcutDisplayTextBox.Text = "";
        InputModifiers = ModifierKeys.None;
        InputKey = Key.None;
        return;
      }

      bool isSpecialKey = IsSpecialKey(key);
      bool hasModifierKey = modifiers != ModifierKeys.None;

      // 특수키만 있는 경우
      if (!hasModifierKey && isSpecialKey)
      {
        var keyName = GetKeyName(key);
        ShortcutDisplayTextBox.Text = keyName;
        InputModifiers = ModifierKeys.None;
        InputKey = key;
        e.Handled = true;
        return;
      }

      // 수정자 키가 있는 경우 (Ctrl, Alt, Shift, Win 중 하나 이상)
      if (hasModifierKey)
      {
        var hotKeyText = BuildHotKeyText(modifiers, key);
        ShortcutDisplayTextBox.Text = hotKeyText;
        InputModifiers = modifiers;
        InputKey = key;
        e.Handled = true;
        return;
      }

      // 단일 문자는 거부
      if (!isSpecialKey)
      {
        ShortcutDisplayTextBox.Text = "";
        InputModifiers = ModifierKeys.None;
        InputKey = Key.None;
        return;
      }
    }

    #endregion
  }
}
