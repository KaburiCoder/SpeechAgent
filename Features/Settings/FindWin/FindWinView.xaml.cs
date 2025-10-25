using System.Windows;
using System.Windows.Input;

namespace SpeechAgent.Features.Settings.FindWin
{
  /// <summary>
  /// FindWinView.xaml에 대한 상호 작용 논리
  /// </summary>
  public partial class FindWinView : Window
  {
    public FindWinView()
    {
      InitializeComponent();
      DataContext = new FindWinViewModel();
    }

    private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        var viewModel = (FindWinViewModel)DataContext;
        viewModel.Search();
      }
    }
  }
}
