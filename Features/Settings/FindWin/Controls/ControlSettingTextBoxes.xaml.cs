using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SpeechAgent.Features.Settings.FindWin.Controls
{
  /// <summary>
  /// ControlSettingTextBoxes.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class ControlSettingTextBoxes : System.Windows.Controls.UserControl
  {
    #region DependencyProperty

    public string FirstHint
    {
      get { return (string)GetValue(FirstHintProperty); }
      set { SetValue(FirstHintProperty, value); }
    }

    public static readonly DependencyProperty FirstHintProperty =
 DependencyProperty.Register("FirstHint", typeof(string), typeof(ControlSettingTextBoxes), new PropertyMetadata("첫 번째 컨트롤"));

    public string FirstControlType
    {
      get { return (string)GetValue(FirstControlTypeProperty); }
    set { SetValue(FirstControlTypeProperty, value); }
    }

   public static readonly DependencyProperty FirstControlTypeProperty =
        DependencyProperty.Register("FirstControlType", typeof(string), typeof(ControlSettingTextBoxes), 
 new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string FirstIndex
    {
   get { return (string)GetValue(FirstIndexProperty); }
      set { SetValue(FirstIndexProperty, value); }
    }

    public static readonly DependencyProperty FirstIndexProperty =
        DependencyProperty.Register("FirstIndex", typeof(string), typeof(ControlSettingTextBoxes), 
          new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string SecondHint
    {
      get { return (string)GetValue(SecondHintProperty); }
      set { SetValue(SecondHintProperty, value); }
    }

    public static readonly DependencyProperty SecondHintProperty =
        DependencyProperty.Register("SecondHint", typeof(string), typeof(ControlSettingTextBoxes), 
     new PropertyMetadata("두 번째 컨트롤"));

    public string SecondControlType
    {
      get { return (string)GetValue(SecondControlTypeProperty); }
      set { SetValue(SecondControlTypeProperty, value); }
    }

    public static readonly DependencyProperty SecondControlTypeProperty =
        DependencyProperty.Register("SecondControlType", typeof(string), typeof(ControlSettingTextBoxes), 
          new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string SecondIndex
    {
      get { return (string)GetValue(SecondIndexProperty); }
      set { SetValue(SecondIndexProperty, value); }
    }

    public static readonly DependencyProperty SecondIndexProperty =
        DependencyProperty.Register("SecondIndex", typeof(string), typeof(ControlSettingTextBoxes), 
          new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion

    public ControlSettingTextBoxes()
    {
 InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      // 첫 번째 인덱스 TextBox에 PreviewTextInput 이벤트 연결
      FirstIndexTextBox.PreviewTextInput += Index_PreviewTextInput;
 
    // 두 번째 인덱스 TextBox에 PreviewTextInput 이벤트 연결
      SecondIndexTextBox.PreviewTextInput += Index_PreviewTextInput;
    }

    private void Index_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
 // 숫자만 입력 허용
  e.Handled = !IsNumeric(e.Text);
    }

    private bool IsNumeric(string text)
    {
      return int.TryParse(text, out _);
    }
}
}
