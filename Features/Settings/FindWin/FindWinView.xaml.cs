using System.Windows;

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
  }
}
