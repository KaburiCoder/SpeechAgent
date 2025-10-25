using System.Windows;
using System.Windows.Input;

namespace SpeechAgent.Features.Settings
{
  /// <summary>
  /// SettingsView.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class SettingsView : Window
  {
    public SettingsView()
    {
      InitializeComponent();
    }

    // 커스텀 타이틀바 이벤트 핸들러들
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      if (e.ButtonState == MouseButtonState.Pressed)
      {
        this.DragMove();
      }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }

    private void Index_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      e.Handled = !int.TryParse(e.Text, out _);
    }
  }
}
