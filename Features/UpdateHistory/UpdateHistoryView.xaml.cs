using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SpeechAgent.Features.UpdateHistory;

namespace SpeechAgent.Features.UpdateHistory
{
  public partial class UpdateHistoryView : Window
  {
    public UpdateHistoryView()
    {
      InitializeComponent();
    }

    private void OnHeaderMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      // 헤더 영역에서 드래그로 윈도우 이동
      if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
      {
        DragMove();
      }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void OnMarkdownScrollViewerMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (sender is ScrollViewer scrollViewer)
      {
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
        e.Handled = true;
      }
    }
  }
}
